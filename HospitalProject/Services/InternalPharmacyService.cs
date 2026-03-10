using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HospitalProject.Services
{
    public class InternalPharmacyService
    {
        private readonly IRepository<InternalPharmacy> _pharmacy;
        private readonly IRepository<InternalPharmacyStaffRequest> _staffRequest;
        private readonly IRepository<User> _user;
        private readonly IRepository<Medicine> _medicine;
        private readonly IRepository<InternalPharmacyInventory> _inventory;
        private readonly IRepository<Prescription> _prescription;
        private readonly IRepository<DispenseRecord> _dispense;
        private readonly IRepository<PharmacyNotification> _notification;
        private readonly IRepository<DrugInteraction> _interaction;
        private readonly IRepository<PrescriptionItem> _prescriptionItem;

        public InternalPharmacyService(
            IRepository<InternalPharmacy> pharmacy,
            IRepository<InternalPharmacyStaffRequest> staffRequest,
            IRepository<User> user,
            IRepository<Medicine> medicine,
            IRepository<InternalPharmacyInventory> inventory,
            IRepository<Prescription> prescription,
            IRepository<PrescriptionItem> prescriptionItem,
            IRepository<DispenseRecord> dispense,
            IRepository<PharmacyNotification> notification,
            IRepository<DrugInteraction> interaction)
        {
            _pharmacy = pharmacy;
            _staffRequest = staffRequest;
            _user = user;
            _medicine = medicine;
            _inventory = inventory;
            _prescription = prescription;
            _dispense = dispense;
            _notification = notification;
            _prescriptionItem = prescriptionItem;
            _interaction = interaction;
        }

        // 🔐 ADMIN ONLY + ONE TIME CREATE
        public async Task<string> RegisterInternalPharmacy(
            int hospitalId,   //  come from JWT
            InternalPharmacyCreateDto dto)
        {
            // one Hospital one Pharmacy check
            var exists = await _pharmacy.GetAsync(
                x => x.HospitalId == hospitalId);
            if (exists != null)
                throw new Exception("Internal pharmacy already created for this hospital");

            var pharmacy = new InternalPharmacy
            {
                HospitalId = hospitalId,  // come from JWT
                PharmacyName = dto.PharmacyName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Address = dto.Address.Trim()
            };

            await _pharmacy.AddAsync(pharmacy);
            await _pharmacy.SaveAsync();

            return "Internal pharmacy created successfully";
        }

        public async Task<InternalPharmacy?> GetPharmacy(int hospitalId)
        {
            return await _pharmacy.GetAsync(
                x => x.HospitalId == hospitalId && x.IsActive);
        }


        // =========================
        // STAFF REGISTER (PUBLIC)
        // =========================
        public async Task<string> RegisterStaff(
            InternalPharmacyStaffRegisterDto dto)
        {
            // 1️⃣ Already user check
            var userExists = await _user.GetAsync(x =>
                x.MobileNumber == dto.MobileNumber);

            if (userExists != null)
                throw new Exception("Mobile already registered");

            // 2️⃣ Already pending check
            var pending = await _staffRequest.GetAsync(x =>
                x.MobileNumber == dto.MobileNumber &&
                x.Status == "Pending");

            if (pending != null)
                throw new Exception("Request already pending");

            await _staffRequest.AddAsync(new InternalPharmacyStaffRequest
            {
                Name = dto.Name,
                MobileNumber = dto.MobileNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Status = "Pending"
            });

            await _staffRequest.SaveAsync();

            return "Registration request sent for admin approval";
        }

        // =========================
        // ADMIN APPROVE
        // =========================
        public async Task<string> ApproveStaff(
            int adminUserId,
            string role,
            int requestId)
        {
            if (role != "Admin")
                throw new Exception("Only admin can approve");

            var request = await _staffRequest.GetAsync(x =>
                x.Id == requestId &&
                x.Status == "Pending");

            if (request == null)
                throw new Exception("Request not found");

            // Create user
            await _user.AddAsync(new User
            {
                Name = request.Name,
                MobileNumber = request.MobileNumber,
                Password = request.Password,
                Role = "InternalPharmacyStaff"
            });

            await _user.SaveAsync();

            request.Status = "Approved";
            await _staffRequest.SaveAsync();

            return "Pharmacy staff approved successfully";
        }

        // =========================
        // MEDICINE MASTER
        // =========================
        public async Task AddMedicine(MedicineCreateDto dto)
        {
            var medicine = new Medicine
            {
                GenericName = dto.GenericName.Trim(),
                BrandName = dto.BrandName.Trim(),
                Category = dto.Category.Trim(),
                Unit = dto.Unit.Trim(),
                ReorderLevel = dto.ReorderLevel
            };
            await _medicine.AddAsync(medicine);
            await _medicine.SaveAsync();
        }

        public async Task<List<Medicine>> GetAllMedicines()
        {
            return await _medicine.Query().ToListAsync();
        }

        // =========================
        // INVENTORY MANAGEMENT
        // =========================
        public async Task UpdateInventory(InventoryUpdateDto dto)
        {
            var pharmacy = await _pharmacy.GetAsync(x => true);
            if (pharmacy == null) throw new Exception("Pharmacy not initialized");

            var inventory = new InternalPharmacyInventory
            {
                MedicineId = dto.MedicineId,
                BatchNumber = dto.BatchNumber.Trim(),
                ExpiryDate = dto.ExpiryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                Quantity = dto.Quantity,
                Barcode = dto.Barcode.Trim(),
                Price = dto.Price,
                InternalPharmacyId = pharmacy.Id
            };
            await _inventory.AddAsync(inventory);
            await _inventory.SaveAsync();
        }

        public async Task<List<InventoryViewDto>> GetInventory()
        {
            return await _inventory.Query()
                .Include(x => x.Medicine)
                .Select(x => new InventoryViewDto(
                    x.Id, x.MedicineId, x.Medicine.BrandName, x.BatchNumber,
                    x.ExpiryDate, x.Quantity, x.Barcode, x.Price))
                .ToListAsync();
        }

        // =========================
        // PHARMACY QUEUE
        // =========================
        public async Task<List<PharmacyQueueDto>> GetPendingPrescriptions()
        {
            return await _prescription.Query()
                .Include(x => x.Patient).ThenInclude(p => p.User)
                .Include(x => x.Doctor).ThenInclude(d => d.User)
                .Where(x => x.Status == "Pending" || x.Status == "Partial")
                .OrderBy(x => x.CreatedAt)
                .Select(x => new PharmacyQueueDto(
                    x.Id, x.AppointmentId, x.Patient.User.Name, x.Doctor.User.Name,
                    x.CreatedAt, x.Status))
                .ToListAsync();
        }

        // =========================
        // DISPENSING LOGIC
        // =========================
        public async Task<InventoryViewDto?> GetInventoryByBarcode(string barcode)
        {
            var item = await _inventory.Query()
                .Include(x => x.Medicine)
                .FirstOrDefaultAsync(x => x.Barcode == barcode && x.ExpiryDate > DateTime.UtcNow && x.Quantity > 0);

            if (item == null) return null;

            return new InventoryViewDto(
                item.Id, item.MedicineId, item.Medicine.BrandName, item.BatchNumber,
                item.ExpiryDate, item.Quantity, item.Barcode, item.Price);
        }

        public async Task<List<string>> VerifyDrugInteractions(int prescriptionId, List<int> dispensingMedicineIds)
        {
            var results = new List<string>();

            // 1. Get existing interactions from DB that match these IDs
            var rules = await _interaction.Query()
                .Where(x => (dispensingMedicineIds.Contains(x.MedicineIdA) && dispensingMedicineIds.Contains(x.MedicineIdB)))
                .Include(x => x.MedicineA)
                .Include(x => x.MedicineB)
                .ToListAsync();

            foreach (var rule in rules)
            {
                results.Add($"Interaction Found: {rule.MedicineA.BrandName} + {rule.MedicineB.BrandName}. Severity: {rule.Severity}. Note: {rule.Description}");
            }

            return results;
        }

        public async Task AddDrugInteraction(DrugInteractionCreateDto dto)
        {
            await _interaction.AddAsync(new DrugInteraction
            {
                MedicineIdA = dto.MedicineIdA,
                MedicineIdB = dto.MedicineIdB,
                Severity = dto.Severity,
                Description = dto.Description
            });
            await _interaction.SaveAsync();
        }

        public async Task<List<DrugInteractionViewDto>> GetAllInteractions()
        {
            return await _interaction.Query()
                .Include(x => x.MedicineA)
                .Include(x => x.MedicineB)
                .Select(x => new DrugInteractionViewDto(
                    x.Id, x.MedicineA.BrandName, x.MedicineB.BrandName, x.Severity, x.Description
                ))
                .ToListAsync();
        }

        public async Task<int> Dispense(int pharmacistUserId, DispenseRequestDto dto)
        {
            var prescription = await _prescription.Query()
                .Include(x => x.Items).ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(x => x.Id == dto.PrescriptionId);

            if (prescription == null) throw new Exception("Prescription not found");

            // 👇 Refill limit check
            int allowedDispenses = prescription.MaxRefills + 1;
            var existingDispenseCount = await _dispense.Query()
                .CountAsync(x => x.PrescriptionId == dto.PrescriptionId);

            if (existingDispenseCount >= allowedDispenses)
                throw new Exception($"Dispense limit reached. Allowed only {allowedDispenses} time(s)");

            // 1. Safety Check: Drug Interactions
            var medIds = dto.Items.Select(i => i.MedicineId).ToList();
            var safetyIssues = await VerifyDrugInteractions(dto.PrescriptionId, medIds);
            if (safetyIssues.Any()) throw new Exception($"Safety Alert: {string.Join(", ", safetyIssues)}");

            var dispenseRecord = new DispenseRecord
            {
                PrescriptionId = dto.PrescriptionId,
                PharmacistId = pharmacistUserId,
                Remarks = dto.Remarks,
                TotalAmount = 0
            };

            foreach (var item in dto.Items)
            {
                var presItem = prescription.Items.FirstOrDefault(x => x.MedicineId == item.MedicineId);

                // Generic Substitution Check
                if (presItem == null)
                {
                    var substituteMed = await _medicine.GetAsync(x => x.Id == item.MedicineId);
                    if (substituteMed == null) continue;

                    var originalItem = prescription.Items.FirstOrDefault(i => i.Medicine.GenericName == substituteMed.GenericName);

                    if (originalItem != null && originalItem.GenericSubstitutionAllowed)
                    {
                        presItem = originalItem;
                    }
                    else
                    {
                        throw new Exception($"Medicine {substituteMed.BrandName} is not in prescription and substitution is not approved.");
                    }
                }

                // FEFO: First Expiry First Out
                var inventoryQuery = _inventory.Query()
                    .Where(x => x.MedicineId == item.MedicineId && x.ExpiryDate > DateTime.UtcNow && x.Quantity > 0);

                if (!string.IsNullOrEmpty(item.Barcode))
                {
                    inventoryQuery = inventoryQuery.Where(x => x.Barcode == item.Barcode);
                }

                var batches = await inventoryQuery.OrderBy(x => x.ExpiryDate).ToListAsync();

                int remainingToDispense = item.QuantityDispensed;
                foreach (var batch in batches)
                {
                    if (remainingToDispense <= 0) break;

                    int take = Math.Min(batch.Quantity, remainingToDispense);
                    batch.Quantity -= take;
                    remainingToDispense -= take;

                    dispenseRecord.Items.Add(new DispenseItem
                    {
                        MedicineId = item.MedicineId,
                        BatchNumber = batch.BatchNumber,
                        QuantityDispensed = take,
                        PricePerUnit = batch.Price
                    });

                    dispenseRecord.TotalAmount += (take * batch.Price);
                }

                if (remainingToDispense > 0)
                {
                    await _notification.AddAsync(new PharmacyNotification
                    {
                        PatientId = prescription.PatientId,
                        Message = $"Item {item.MedicineId} is out of stock. {remainingToDispense} units are pending.",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                presItem.QuantityDispensed += (item.QuantityDispensed - remainingToDispense);
            }

            // Update prescription status
            bool allFullyDispensed = prescription.Items.All(x => x.QuantityDispensed >= x.QuantityPrescribed);
            prescription.Status = allFullyDispensed ? "Dispensed" : "Partial";

            await _dispense.AddAsync(dispenseRecord);
            await _prescription.SaveAsync();
            await _prescriptionItem.SaveAsync();
            await _inventory.SaveAsync();
            await _dispense.SaveAsync();
            await _notification.SaveAsync();

            return dispenseRecord.Id;
        }

        public async Task<List<PharmacyNotificationDto>> GetPatientNotifications(int patientId)
        {
            return await _notification.Query()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new PharmacyNotificationDto(x.Id, x.Message, x.CreatedAt, x.IsRead))
                .ToListAsync();
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            var notif = await _notification.GetAsync(x => x.Id == notificationId);
            if (notif != null)
            {
                notif.IsRead = true;
                await _notification.SaveAsync();
            }
        }

        public async Task<PatientDispenseHistoryDto?> GetDispenseReceipt(int dispenseId)
        {
            var x = await _dispense.Query()
                .Include(x => x.Prescription)
                .Include(x => x.Pharmacist)
                .Include(x => x.Items).ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(x => x.Id == dispenseId);

            if (x == null) return null;

            return new PatientDispenseHistoryDto(
                x.Id,
                x.DispensedAt,
                x.Pharmacist.Name,
                x.Remarks ?? string.Empty,
                x.TotalAmount,
                x.Items.Select(i => new DispenseHistoryItemDto(
                    i.Medicine.BrandName,
                    i.BatchNumber,
                    i.QuantityDispensed,
                    i.PricePerUnit
                )).ToList()
            );
        }

        public async Task<List<LowStockAlertDto>> GetLowStockAlerts()
        {
            // 1️⃣ Get raw grouped data from DB
            var data = await _inventory.Query()
                .Include(x => x.Medicine)
                .GroupBy(x => new 
                { 
                    x.MedicineId, 
                    x.Medicine.BrandName, 
                    x.Medicine.ReorderLevel 
                })
                .Select(g => new 
                {
                    g.Key.MedicineId,
                    MedicineName = g.Key.BrandName,
                    CurrentStock = g.Sum(x => x.Quantity),
                    ReorderLevel = g.Key.ReorderLevel
                })
                .ToListAsync();

            // 2️⃣ Filter and project in memory to avoid translation issues
            return data
                .Where(x => x.CurrentStock < x.ReorderLevel)
                .Select(x => new LowStockAlertDto(
                    x.MedicineId,
                    x.MedicineName,
                    x.CurrentStock,
                    x.ReorderLevel
                ))
                .ToList();
        }

        public async Task<List<MedicineStockDto>> GetAvailableMedicines(string? query = null)
        {
            var medsQuery = _medicine.Query();
            if (!string.IsNullOrEmpty(query))
            {
                medsQuery = medsQuery.Where(x =>
                    x.BrandName.Contains(query) ||
                    x.GenericName.Contains(query));
            }

            var meds = await medsQuery.ToListAsync();

            var stocks = await _inventory.Query()
                .Where(x => x.ExpiryDate > DateTime.UtcNow)
                .GroupBy(x => x.MedicineId)
                .Select(g => new { MedicineId = g.Key, TotalStock = g.Sum(x => x.Quantity) })
                .ToListAsync();

            var stockMap = stocks.ToDictionary(x => x.MedicineId, x => x.TotalStock);

            return meds.Select(m => new MedicineStockDto(
                m.Id,
                m.GenericName,
                m.BrandName,
                m.Category,
                m.Unit,
                stockMap.ContainsKey(m.Id) ? stockMap[m.Id] : 0
            )).ToList();
        }

        public async Task<List<PatientDispenseHistoryDto>> GetPatientDispenseHistory(int patientId)
        {
            return await _dispense.Query()
                .Include(x => x.Prescription)
                .Include(x => x.Pharmacist)
                .Include(x => x.Items).ThenInclude(i => i.Medicine)
                .Where(x => x.Prescription.PatientId == patientId)
                .OrderByDescending(x => x.Id)
                .Select(x => new PatientDispenseHistoryDto(
                    x.Id,
                    x.DispensedAt, 
                    x.Pharmacist.Name,
                    x.Remarks ?? string.Empty,
                    x.TotalAmount,
                    x.Items.Select(i => new DispenseHistoryItemDto(
                        i.Medicine.BrandName,
                        i.BatchNumber,
                        i.QuantityDispensed,
                        i.PricePerUnit
                    )).ToList()
                ))
                .ToListAsync();
        }
    }
}
