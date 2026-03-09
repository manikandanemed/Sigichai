using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Text.Json;

namespace HospitalProject.Services
{
    public class PrescriptionQrService
    {
        private readonly IRepository<Prescription> _prescription;
        private readonly IRepository<PrescriptionQrCode> _qrCode;
        private readonly IRepository<QrScanLog> _scanLog;
        private readonly IRepository<User> _user;

        public PrescriptionQrService(
            IRepository<Prescription> prescription,
            IRepository<PrescriptionQrCode> qrCode,
            IRepository<QrScanLog> scanLog,
            IRepository<User> user)
        {
            _prescription = prescription;
            _qrCode = qrCode;
            _scanLog = scanLog;
            _user = user;
        }

        // =====================================================================
        // 1️⃣ GENERATE QR — Doctor consult முடிஞ்சதும் auto call ஆகும்
        // =====================================================================
        public async Task GenerateQrForPrescription(int prescriptionId)
        {
            // Already exists check
            var existing = await _qrCode.Query()
                .FirstOrDefaultAsync(q => q.PrescriptionId == prescriptionId);
            if (existing != null) return;

            // Load prescription
            var prescription = await _prescription.Query()
                .Include(p => p.Patient).ThenInclude(pat => pat.User)
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Items).ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId)
                ?? throw new Exception("Prescription not found");

            var validityDays = prescription.ValidityDays > 0 ? prescription.ValidityDays : 30;
            var validUntil = DateTime.UtcNow.AddDays(validityDays);

            var payload = new QrPayloadDto
            {
                PrescriptionId = prescription.Id,
                PatientId = prescription.PatientId,
                PatientName = prescription.Patient.User.Name,
                ValidUntil = validUntil,
                MaxRefills = prescription.MaxRefills,
                Medicines = prescription.Items.Select(i => new QrMedicineDto
                {
                    MedicineId = i.MedicineId,
                    GenericName = i.Medicine.GenericName,
                    BrandName = i.Medicine.BrandName,
                    Dosage = i.Dosage,
                    Duration = i.Duration,
                    QuantityPrescribed = i.QuantityPrescribed,
                    GenericSubstitutionAllowed = i.GenericSubstitutionAllowed
                }).ToList()
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var qrImageBase64 = GenerateQrBase64(payloadJson);

            var qrRecord = new PrescriptionQrCode
            {
                PrescriptionId = prescription.Id,
                QrPayload = payloadJson,
                QrImageBase64 = qrImageBase64,
                ValidUntil = validUntil,
                MaxRefills = prescription.MaxRefills,
                UsedRefills = 0,
                IsFullyUsed = false,
                GeneratedAt = DateTime.UtcNow
            };

            await _qrCode.AddAsync(qrRecord);
            await _qrCode.SaveAsync();
        }

        // =====================================================================
        // 2️⃣ PATIENT — Own QR view
        // =====================================================================
        public async Task<PrescriptionQrViewDto> GetQrForPatient(int prescriptionId, int userId)
        {
            var user = await _user.Query()
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("User not found");

            if (user.Patient == null)
                throw new Exception("Patient profile not found");

            var prescription = await _prescription.Query()
                .Include(p => p.Doctor).ThenInclude(d => d.User)
                .Include(p => p.Patient).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == prescriptionId && p.PatientId == user.Patient.Id)
                ?? throw new Exception("Prescription not found or access denied");

            var qr = await _qrCode.Query()
                .FirstOrDefaultAsync(q => q.PrescriptionId == prescriptionId)
                ?? throw new Exception("QR code not yet generated");

            return new PrescriptionQrViewDto(
                PrescriptionId: qr.PrescriptionId,
                PatientName: prescription.Patient.User.Name,
                DoctorName: prescription.Doctor.User.Name,
                GeneratedAt: qr.GeneratedAt,
                ValidUntil: qr.ValidUntil,
                MaxRefills: qr.MaxRefills,
                UsedRefills: qr.UsedRefills,
                IsFullyUsed: qr.IsFullyUsed,
                QrImageBase64: qr.QrImageBase64
            );
        }

        // =====================================================================
        // 3️⃣ PHARMACY — QR Scan (Internal & External)
        // =====================================================================
        public async Task<QrScanResultDto> ScanQrCode(string rawQrPayload, int scannedByUserId, string pharmacyName)
        {
            // Parse payload
            QrPayloadDto payload;
            try
            {
                payload = JsonSerializer.Deserialize<QrPayloadDto>(rawQrPayload)
                    ?? throw new Exception("Invalid QR data");
            }
            catch
            {
                return new QrScanResultDto(false, "Invalid QR format",
                    0, 0, "", "", DateTime.MinValue, 0, 0, new());
            }

            // Find QR record
            var qr = await _qrCode.Query()
                .Include(q => q.Prescription)
                    .ThenInclude(p => p.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(q => q.PrescriptionId == payload.PrescriptionId);

            if (qr == null)
                return new QrScanResultDto(false, "QR not found",
                    payload.PrescriptionId, payload.PatientId, payload.PatientName,
                    "", payload.ValidUntil, 0, 0, new());

            var doctorName = qr.Prescription.Doctor.User.Name;

            // Expiry check
            if (DateTime.UtcNow > qr.ValidUntil)
            {
                await LogScan(qr.Id, scannedByUserId, pharmacyName, false, "Expired", qr.UsedRefills + 1);
                return new QrScanResultDto(false, "Prescription expired",
                    payload.PrescriptionId, payload.PatientId, payload.PatientName,
                    doctorName, qr.ValidUntil, 0, qr.UsedRefills + 1, payload.Medicines);
            }

            // Refill check
            if (qr.IsFullyUsed)
            {
                await LogScan(qr.Id, scannedByUserId, pharmacyName, false, "NoRefillsLeft", qr.UsedRefills + 1);
                return new QrScanResultDto(false, "No refills remaining",
                    payload.PrescriptionId, payload.PatientId, payload.PatientName,
                    doctorName, qr.ValidUntil, 0, qr.UsedRefills + 1, payload.Medicines);
            }

            // ✅ Valid — increment usage
            var refillNumber = qr.UsedRefills + 1;
            qr.UsedRefills += 1;
            var refillsRemaining = qr.MaxRefills - qr.UsedRefills;

            if (qr.UsedRefills > qr.MaxRefills)
                qr.IsFullyUsed = true;

            await _qrCode.SaveAsync();
            await LogScan(qr.Id, scannedByUserId, pharmacyName, true, null, refillNumber);

            return new QrScanResultDto(
                IsValid: true,
                InvalidReason: null,
                PrescriptionId: payload.PrescriptionId,
                PatientId: payload.PatientId,
                PatientName: payload.PatientName,
                DoctorName: doctorName,
                ValidUntil: qr.ValidUntil,
                RefillsRemaining: Math.Max(0, refillsRemaining),
                RefillNumber: refillNumber,
                Medicines: payload.Medicines
            );
        }

        // =====================================================================
        // 4️⃣ AUDIT LOG VIEW
        // =====================================================================
        public async Task<List<QrScanLogViewDto>> GetScanLogs(int prescriptionId)
        {
            return await _scanLog.Query()
                .Include(l => l.ScannedByUser)
                .Where(l => l.PrescriptionQrCode.PrescriptionId == prescriptionId)
                .OrderByDescending(l => l.ScannedAt)
                .Select(l => new QrScanLogViewDto(
                    l.Id,
                    l.PrescriptionQrCode.PrescriptionId,
                    l.PharmacyName,
                    l.ScannedByUser.Name,
                    l.WasValid,
                    l.InvalidReason,
                    l.RefillNumber,
                    l.ScannedAt
                ))
                .ToListAsync();
        }

        // =====================================================================
        // 🔧 PRIVATE HELPERS
        // =====================================================================
        private static string GenerateQrBase64(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var pngBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(pngBytes);
        }

        private async Task LogScan(int qrId, int userId, string pharmacyName, bool valid, string? reason, int refillNum)
        {
            var log = new QrScanLog
            {
                PrescriptionQrCodeId = qrId,
                ScannedByUserId = userId,
                PharmacyName = pharmacyName,
                WasValid = valid,
                InvalidReason = reason,
                RefillNumber = refillNum,
                ScannedAt = DateTime.UtcNow
            };

            await _scanLog.AddAsync(log);
            await _scanLog.SaveAsync();
        }
    }
}