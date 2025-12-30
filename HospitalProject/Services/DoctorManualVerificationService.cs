using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalProject.Services
{
    public class DoctorManualVerificationService
    {
        private readonly IRepository<Doctor> _doctor;
        private readonly IRepository<DoctorVerification> _verification;
        private readonly IRepository<User> _user;
        private readonly ITwilioService _twilio;

        public DoctorManualVerificationService(
            IRepository<Doctor> doctor,
            IRepository<DoctorVerification> verification,
            IRepository<User> user,
            ITwilioService twilio)
        {
            _doctor = doctor;
            _verification = verification;
            _user = user;
            _twilio = twilio;
        }

        // =====================================================
        // 1️⃣ Doctor submits verification
        // =====================================================
        public async Task SubmitVerificationAsync(
            int doctorUserId,
            DoctorVerificationDto dto)
        {
            // Doctor
            var doctor = await _doctor.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // User
            var user = await _user.GetAsync(u => u.Id == doctorUserId);
            if (user == null)
                throw new Exception("User not found");

            // Save verification request
            await _verification.AddAsync(new DoctorVerification
            {
                DoctorId = doctor.Id,
                RegistrationNumber = dto.RegistrationNumber,
                YearOfRegistration = dto.YearOfRegistration,
                CouncilName = dto.CouncilName,
                VerificationStatus = "PENDING",
                VerifiedOn = DateTime.UtcNow
            });

            // Doctor still NOT verified
            doctor.IsVerified = false;

            await _verification.SaveAsync();
            await _doctor.SaveAsync();

            // SMS
            await _twilio.SendOtpAsync(
                user.MobileNumber,
                "Your verification details are submitted. Admin approval may take up to 4 hours."
            );
        }

        // =====================================================
        // Admin – View doctor verification details
        // =====================================================
        public async Task<AdminDoctorVerificationViewDto>
            GetDoctorVerificationDetails(int doctorId)
        {
            var doctor = await _doctor.GetAsync(d => d.Id == doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var user = await _user.GetAsync(u => u.Id == doctor.UserId);
            if (user == null)
                throw new Exception("User not found");

            var verification = await _verification.Query()
                .Where(v => v.DoctorId == doctorId)
                .OrderByDescending(v => v.VerifiedOn)
                .FirstOrDefaultAsync();

            if (verification == null)
                throw new Exception("Verification details not found");

            return new AdminDoctorVerificationViewDto(
                doctor.Id,
                user.Name,
                user.MobileNumber,
                doctor.Specialization,
                verification.RegistrationNumber,
                verification.YearOfRegistration,
                verification.CouncilName,
                verification.VerificationStatus
            );
        }

        // =====================================================
        // 2️⃣ Admin approves / rejects doctor
        // =====================================================
        public async Task ApproveDoctorAsync(
            int doctorId,
            bool approve)
        {
            var doctor = await _doctor.GetAsync(d => d.Id == doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var user = await _user.GetAsync(u => u.Id == doctor.UserId);
            if (user == null)
                throw new Exception("User not found");

            // Final doctor state
            doctor.IsVerified = approve;

            var verification = await _verification.Query()
                .Where(v => v.DoctorId == doctorId)
                .OrderByDescending(v => v.VerifiedOn)
                .FirstOrDefaultAsync();

            if (verification != null)
            {
                verification.VerificationStatus =
                    approve ? "VERIFIED" : "REJECTED";
                verification.VerifiedOn = DateTime.UtcNow;
            }

            await _doctor.SaveAsync();
            await _verification.SaveAsync();

            await _twilio.SendOtpAsync(
                user.MobileNumber,
                approve
                    ? "Your doctor profile has been approved. You can now start consulting."
                    : "Your doctor verification was rejected. Please update details and resubmit."
            );
        }

        // =====================================================
        // 3️⃣ Admin – Pending doctors list
        // =====================================================
        public List<object> GetPendingDoctors(int hospitalId)
        {
            return _verification.Query()
                .Include(v => v.Doctor)
                .ThenInclude(d => d.User)
                .Where(v =>
                    v.VerificationStatus == "PENDING" &&
                    v.Doctor.HospitalId == hospitalId)
                .Select(v => new
                {
                    v.Doctor.Id,
                    DoctorName = v.Doctor.User.Name,
                    v.Doctor.User.MobileNumber,
                    v.Doctor.Specialization,
                    v.VerificationStatus
                })
                .Cast<object>()
                .ToList();
        }
    }
}
