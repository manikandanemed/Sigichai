using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalProject.Services
{
    public class HospitalService
    {
        private readonly IRepository<User> _u;
        private readonly IRepository<Patient> _p;
        private readonly IRepository<Doctor> _d;
        private readonly IRepository<Admin> _admin;
        private readonly IRepository<DoctorAvailability> _slots;
        private readonly IRepository<DoctorProfile> _doctorProfile;
        private readonly IRepository<DoctorDocument> _doctorDocument;
        private readonly IRepository<Appointment> _apps;
        private readonly IRepository<OtpStore> _otp;
        private readonly IRepository<DoctorStaff> _doctorStaff;
        private readonly ITwilioService _twilio;
        private readonly IConfiguration _config;
        private readonly IRepository<FamilyMember> _family;
        private readonly IRepository<Hospital> _hospital;



        public HospitalService(
            IRepository<User> u,
            IRepository<Patient> p,
            IRepository<Doctor> d,
            IRepository<Admin> admin,
            IRepository<DoctorAvailability> slots,
            IRepository<Appointment> apps,
            IRepository<OtpStore> otp,
            IRepository<FamilyMember> family,   // 👈 ADD
            IRepository<Hospital> hospital,
            IRepository<DoctorProfile> doctorProfile,      // 👈 ADD
            IRepository<DoctorDocument> doctorDocument,    // 👈 ADD
            IRepository<DoctorStaff> doctorStaff,
            ITwilioService twilio,
            IConfiguration config)
        {
            _u = u; _p = p; _d = d; _admin = admin;
            _slots = slots; _apps = apps; _otp = otp; _family = family; _hospital = hospital;
            _doctorProfile = doctorProfile;        // 👈
            _doctorDocument = doctorDocument;      // 👈
            _doctorStaff = doctorStaff;
            _twilio = twilio; _config = config;
        }

        // =========================
        // REGISTRATIONS
        // =========================
        public async Task RegisterPatient(PatientRegDto d)
        {
            if (await _u.GetAsync(x => x.MobileNumber == d.Mobile) != null)
                throw new Exception("Mobile already exists");

            var user = new User
            {
                Name = d.Name,
                MobileNumber = d.Mobile,
                Password = BCrypt.Net.BCrypt.HashPassword(d.Password),
                Role = "Patient",
                Latitude = d.Lat,
                Longitude = d.Lon
            };

            await _u.AddAsync(user);
            await _u.SaveAsync();

            await _p.AddAsync(new Patient { UserId = user.Id });
            await _p.SaveAsync();
        }


        public async Task<int> CreateHospital(HospitalCreateDto dto)
        {
            // Optional: duplicate name check
            var exists = await _hospital.GetAsync(
                h => h.Name.ToLower() == dto.Name.ToLower());

            if (exists != null)
                throw new Exception("Hospital already exists");

            var hospital = new Hospital
            {
                Name = dto.Name.Trim(),
                Address = dto.Address.Trim(),
                Phone = dto.Phone.Trim()
            };

            await _hospital.AddAsync(hospital);
            await _hospital.SaveAsync();

            return hospital.Id; // 🔑 IMPORTANT
        }




        public async Task RegisterDoctor(int hospitalIdFromJwt, DoctorRegDto d)
        {
            // 1️⃣ Duplicate mobile check
            if (await _u.GetAsync(x => x.MobileNumber == d.Mobile) != null)
                throw new Exception("Mobile already exists");

            // 2️⃣ Create User
            var user = new User
            {
                Name = d.Name,
                MobileNumber = d.Mobile,
                Password = BCrypt.Net.BCrypt.HashPassword(d.Password),
                Role = "Doctor",
                Latitude = d.Lat,
                Longitude = d.Lon
            };

            await _u.AddAsync(user);
            await _u.SaveAsync();

            // 3️⃣ Create Doctor (HospitalId AUTO from JWT)
            await _d.AddAsync(new Doctor
            {
                UserId = user.Id,
                HospitalId = hospitalIdFromJwt, // 🔥 AUTO ASSIGN
                Specialization = d.Specialization
            });

            await _d.SaveAsync();
        }



        public async Task RegisterAdmin(int hospitalIdFromJwt, AdminRegDto d)
        {
            // 1️⃣ Duplicate mobile check
            if (await _u.GetAsync(x => x.MobileNumber == d.Mobile) != null)
                throw new Exception("Mobile already exists");

            // 2️⃣ Create User
            var user = new User
            {
                Name = d.Name,
                MobileNumber = d.Mobile,
                Password = BCrypt.Net.BCrypt.HashPassword(d.Password),
                Role = "Admin"
            };

            await _u.AddAsync(user);
            await _u.SaveAsync();

            // 3️⃣ Create Admin (HospitalId AUTO)
            await _admin.AddAsync(new Admin
            {
                UserId = user.Id,
                HospitalId = hospitalIdFromJwt // 🔥 IMPORTANT
            });

            await _admin.SaveAsync();
        }


        // =========================
        // LOGIN + OTP
        // =========================
        public async Task<string> Login(LoginDto d)
        {
            //var user = await _u.GetAsync(x => x.MobileNumber == d.MobileNumber);
            var user = await _u.Query()
    .Include(x => x.Admin)
    .Include(x => x.Doctor)
    .Include(x => x.Patient)
    .FirstOrDefaultAsync(x => x.MobileNumber == d.MobileNumber);
            if (user == null || !BCrypt.Net.BCrypt.Verify(d.Password, user.Password))
                return "Invalid Credentials";

            var otp = new Random().Next(1000, 9999).ToString();

            await _otp.AddAsync(new OtpStore
            {
                MobileNumber = d.MobileNumber,
                OtpCode = otp,
                Expiry = DateTime.UtcNow.AddMinutes(5)
            });
            await _otp.SaveAsync();

            await _twilio.SendOtpAsync(d.MobileNumber, otp);
            return "OTP Sent";
        }

 


        public async Task<string> VerifyOtp(VerifyOtpDto d)
        {
            var rec = await _otp.GetAsync(x =>
                x.MobileNumber == d.MobileNumber &&
                x.OtpCode == d.Otp &&
                x.Expiry > DateTime.UtcNow);

            if (rec == null)
                throw new Exception("Invalid OTP");

            // 🔥 INCLUDE navigation properties
            var user = await _u.Query()
                .Include(x => x.Admin)
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .FirstOrDefaultAsync(x => x.MobileNumber == d.MobileNumber);

            if (user == null)
                throw new Exception("User not found");

            return GenerateJwt(user);
        }

        // =========================
        // FORGET PASSWORD
        // =========================


        public async Task ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _u.GetAsync(x => x.MobileNumber == dto.MobileNumber);
            if (user == null)
                throw new Exception("User not found");

            var otp = new Random().Next(1000, 9999).ToString();

            await _otp.AddAsync(new OtpStore
            {
                MobileNumber = dto.MobileNumber,
                OtpCode = otp,
                Expiry = DateTime.UtcNow.AddMinutes(5)
            });

            await _otp.SaveAsync();

            await _twilio.SendOtpAsync(dto.MobileNumber, otp);
        }

        // =========================
        // RESET PASSWORD
        // =========================


        public async Task ResetPassword(ResetPasswordDto dto)
        {
            var otpRec = await _otp.GetAsync(x =>
                x.MobileNumber == dto.MobileNumber &&
                x.OtpCode == dto.Otp &&
                x.Expiry > DateTime.UtcNow);

            if (otpRec == null)
                throw new Exception("Invalid or expired OTP");

            var user = await _u.GetAsync(x => x.MobileNumber == dto.MobileNumber);
            if (user == null)
                throw new Exception("User not found");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _u.SaveAsync();
        }



      

        // =========================
        // SLOT ADD (Doctor / Admin)
        // =========================
        public async Task AddDoctorSlot(int doctorId, SlotCreateDto dto)
        {
            // 1️⃣ Doctor exists check
            var doctor = await _d.GetAsync(d => d.Id == doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ 🔴 DOCTOR VERIFICATION CHECK (IMPORTANT)
            if (!doctor.IsVerified)
                throw new Exception("Doctor not verified by medical council");

            // 3️⃣ Convert DateOnly → UTC DateTime
            var utcDate = DateTime.SpecifyKind(
                dto.AvailableDate.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            // 4️⃣ Slot already exists check
            bool exists = _slots.Query().Any(x =>
                x.DoctorId == doctorId &&
                x.AvailableDate == utcDate &&
                x.TimeSlot == dto.TimeSlot
            );

            if (exists)
                throw new Exception("Slot already exists");

            // 5️⃣ Add slot
            await _slots.AddAsync(new DoctorAvailability
            {
                DoctorId = doctorId,
                AvailableDate = utcDate,   // 🔥 ONLY UTC
                TimeSlot = dto.TimeSlot.Trim()
            });

            await _slots.SaveAsync();
        }




        // =========================
        // VIEW SLOTS (Patient)
        // =========================


        public async Task<List<DoctorAvailability>> GetAvailableSlots(
   int doctorId,
   DateOnly date)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            return await _slots.Query()
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.AvailableDate == utcDate)
                .ToListAsync();
        }



        // =========================
        // BOOK APPOINTMENT
        // =========================
        public async Task<string> BookAppointment(int userId, int slotId)
        {
            var patient = await _p.GetAsync(x => x.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            var slot = await _slots.GetAsync(x => x.Id == slotId);
            if (slot == null)
                throw new Exception("Slot not found");

            string tempToken = Guid.NewGuid().ToString("N")[..8].ToUpper();

            await _apps.AddAsync(new Appointment
            {
                PatientId = patient.Id,
                DoctorId = slot.DoctorId,
                AppointmentDate = slot.AvailableDate,
                TimeSlot = slot.TimeSlot,
                TempToken = tempToken,
                Status = "Booked"
            });

            await _apps.SaveAsync();
            return tempToken;
        }

        // =========================
        // SELF BOOKING
        // =========================


        public async Task<string> BookPatientByTime(
    int userId,
    PatientTimeBookingDto dto)
        {
            var patient = await _p.GetAsync(x => x.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            // 🔴 IMPORTANT: Hospital → Doctor validation (HERE)
            var doctor = await _d.GetAsync(x =>
                x.Id == dto.DoctorId &&
                x.HospitalId == dto.HospitalId);

            if (doctor == null)
                throw new Exception("Doctor not belongs to this hospital");

            var utcDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            var slotExists = await _slots.GetAsync(x =>
                x.DoctorId == dto.DoctorId &&
                x.AvailableDate == utcDate &&
                x.TimeSlot == dto.TimeSlot);

            if (slotExists == null)
                throw new Exception("Selected time not available");

            var token = Guid.NewGuid().ToString("N")[..8].ToUpper();

            await _apps.AddAsync(new Appointment
            {
                HospitalId = dto.HospitalId,   // 👈 VERY IMPORTANT
                PatientId = patient.Id,
                DoctorId = dto.DoctorId,
                AppointmentDate = utcDate,
                TimeSlot = dto.TimeSlot,
                TempToken = token,
                Status = "Booked",
                ReasonForVisit = dto.ReasonForVisit   // 👈 ADD
            });

            await _apps.SaveAsync();
            return token;
        }


        // =========================
        // FAMILY BOOKING
        // =========================


        public async Task<string> BookFamilyByTime(
    int userId,
    FamilyTimeBookingDto dto)
        {
            // 1️⃣ Logged-in patient check
            var patient = await _p.GetAsync(x => x.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            // 2️⃣ Family member belongs to this patient check
            var family = await _family.GetAsync(x =>
                x.Id == dto.FamilyMemberId &&
                x.PatientId == patient.Id);

            if (family == null)
                throw new Exception("Invalid family member");

            // 3️⃣ Doctor belongs to selected hospital check (🔥 VERY IMPORTANT)
            var doctor = await _d.GetAsync(x =>
                x.Id == dto.DoctorId &&
                x.HospitalId == dto.HospitalId);

            if (doctor == null)
                throw new Exception("Doctor does not belong to this hospital");

            // 4️⃣ Convert DateOnly → UTC DateTime (00:00)
            var utcDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            // 5️⃣ Check slot availability (doctor + date + time)
            var slotExists = await _slots.GetAsync(x =>
                x.DoctorId == dto.DoctorId &&
                x.AvailableDate == utcDate &&
                x.TimeSlot == dto.TimeSlot);

            if (slotExists == null)
                throw new Exception("Selected time slot not available");

            // 6️⃣ Generate temporary token
            var tempToken = Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpper();

            // 7️⃣ Create appointment for family member
            await _apps.AddAsync(new Appointment
            {
                HospitalId = dto.HospitalId,      // 🔥 Multi-hospital safety
                PatientId = patient.Id,
                FamilyMemberId = family.Id,
                DoctorId = dto.DoctorId,
                AppointmentDate = utcDate,
                TimeSlot = dto.TimeSlot.Trim(),
                TempToken = tempToken,
                Status = "Booked",
                ReasonForVisit = dto.ReasonForVisit   // 👈 ADD
                //IsPaid = false
            });

            await _apps.SaveAsync();

            return tempToken;
        }




        // =========================
        // ADD FAMILY
        // =========================


        public async Task AddFamilyMember(int userId, AddFamilyMemberDto dto)
        {
            var patient = await _p.GetAsync(x => x.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            await _family.AddAsync(new FamilyMember
            {
                PatientId = patient.Id,
                Name = dto.Name,
                Relationship = dto.Relationship
            });

            await _family.SaveAsync();
        }


        // =========================
        // ONLINE BOOKINGS LIST (BOOKED)
        // =========================


        public async Task<List<AppointmentListDto>> GetOnlineBookingsByDate(
    DateOnly date)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            return await _apps.Query()
                .Where(a =>
                    a.AppointmentDate == utcDate &&
                    a.Status == "Booked")
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .OrderBy(a => a.TimeSlot)
                .Select(a => new AppointmentListDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.TimeSlot,
                    a.Status,
                    null   // ❌ queue token not yet
                ))
                .ToListAsync();
        }



        // =========================
        //CHECK-IN LIST + QUEUE
        // ===========================


        public async Task<List<AppointmentListDto>> GetCheckedInAppointmentsByDate(
    DateOnly date)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            return await _apps.Query()
                .Where(a =>
                    a.AppointmentDate == utcDate &&
                    a.Status == "CheckedIn")
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .OrderBy(a => a.QueueToken)
                .Select(a => new AppointmentListDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.TimeSlot,
                    a.Status,
                    a.QueueToken
                ))
                .ToListAsync();
        }






        // =========================
        // CHECK-IN (QR SCAN)
        // =========================
        public async Task<int> CheckIn(string tempToken)
        {
            var app = await _apps.GetAsync(x => x.TempToken == tempToken);
            if (app == null)
                throw new Exception("Invalid token");

            if (app.Status == "CheckedIn")
                return app.QueueToken!.Value;

            int count = _apps.Query().Count(x =>
                x.DoctorId == app.DoctorId &&
                x.AppointmentDate.Date == app.AppointmentDate.Date &&
                x.Status == "CheckedIn");

            app.Status = "CheckedIn";
            app.QueueToken = count + 1;

            await _apps.SaveAsync();
            return app.QueueToken.Value;
        }

        // =========================
        // Doctor – Appointments (Upcoming / Past)
        // =========================


        public async Task<List<DoctorAppointmentDto>> GetDoctorAppointments(
    int userId,
    string type)
        {
            var doctor = await _d.GetAsync(d => d.UserId == userId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var today = DateTime.UtcNow.Date;

            var query = _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Where(a => a.DoctorId == doctor.Id);

            if (type == "upcoming")
                query = query.Where(a => a.AppointmentDate.Date >= today);
            else if (type == "past")
                query = query.Where(a => a.AppointmentDate.Date < today);

            return await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.TimeSlot)
                .Select(a => new DoctorAppointmentDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status,
                    a.QueueToken,
                    a.ReasonForVisit      // 👈 MUST ADD
                ))
                .ToListAsync();
        }


        public async Task AddDoctorProfile(
    int userId,
    DoctorProfileCreateDto dto)
        {
            var doctor = await _d.GetAsync(d => d.UserId == userId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var existingProfile = await _doctorProfile.GetAsync(
                x => x.DoctorId == doctor.Id);

            if (existingProfile != null)
                throw new Exception("Profile already exists");

            var profile = new DoctorProfile
            {
                DoctorId = doctor.Id,
                Dob = dto.Dob,
                Experience = dto.Experience,
                Languages = dto.Languages,
                LicenseType = dto.LicenseType,
                LicenseNumber = dto.LicenseNumber,
                StateCouncil = dto.StateCouncil,
                Degree = dto.Degree,
                University = dto.University,
                GraduationYear = dto.GraduationYear,
                PracticeMode = dto.PracticeMode,
                ConsultationFee = dto.ConsultationFee
            };

            await _doctorProfile.AddAsync(profile);
            await _doctorProfile.SaveAsync();
        }




        // =========================
        // Doctor – Today Queue List (CheckedIn)
        // =========================



        public async Task<List<DoctorAppointmentDto>> GetDoctorQueue(
    int userId,
    DateOnly date)
        {
            var doctor = await _d.GetAsync(d => d.UserId == userId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            return await _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate == utcDate &&
                    a.Status == "CheckedIn")
                .OrderBy(a => a.QueueToken)
                .Select(a => new DoctorAppointmentDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status,
                    a.QueueToken,
                    a.ReasonForVisit      // 👈 MUST ADD
                ))
                .ToListAsync();
        }

     //Doctor create staff
        public async Task CreateStaff(
    int doctorUserId,
    StaffCreateDto dto)
        {
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            if (await _u.GetAsync(x => x.MobileNumber == dto.Mobile) != null)
                throw new Exception("Mobile already exists");

            // Create user
            var user = new User
            {
                Name = dto.Name,
                MobileNumber = dto.Mobile,
                Password = BCrypt.Net.BCrypt.HashPassword("1234"), // temp
                Role = "Staff"
            };

            await _u.AddAsync(user);
            await _u.SaveAsync();

            // Link staff to doctor
            await _doctorStaff.AddAsync(new DoctorStaff
            {
                DoctorId = doctor.Id,
                UserId = user.Id,
                StaffRole = dto.StaffRole
            });

            await _doctorStaff.SaveAsync();

            // Send OTP for first login
            var otp = new Random().Next(1000, 9999).ToString();

            await _otp.AddAsync(new OtpStore
            {
                MobileNumber = dto.Mobile,
                OtpCode = otp,
                Expiry = DateTime.UtcNow.AddMinutes(5)
            });

            await _otp.SaveAsync();
            await _twilio.SendOtpAsync(dto.Mobile, otp);
        }

     // Staff see quee for doctors assigned staffs
        public async Task<List<StaffQueueDto>> GetStaffQueue(
    int staffUserId,
    DateOnly date)
        {
            var staff = await _doctorStaff.GetAsync(
                s => s.UserId == staffUserId);

            if (staff == null)
                throw new Exception("Staff not linked to doctor");

            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            return await _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Where(a =>
                    a.DoctorId == staff.DoctorId &&
                    a.AppointmentDate == utcDate &&
                    a.Status == "CheckedIn")
                .OrderBy(a => a.QueueToken)
                .Select(a => new StaffQueueDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.TimeSlot,
                    a.QueueToken!.Value,
                    a.ReasonForVisit        // 👈
                ))
                .ToListAsync();
        }









        // =========================
        // DOCTOR CONSULT
        // =========================
  
 
        public async Task Consult(DoctorConsultDto d)
        {
            // 1️⃣ Appointment check
            var app = await _apps.GetAsync(x => x.Id == d.AppId);
            if (app == null)
                throw new Exception("Appointment not found");

            // 2️⃣ Doctor fetch
            var doctor = await _d.GetAsync(x => x.Id == app.DoctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 3️⃣ 🔴 DOCTOR VERIFICATION CHECK (IMPORTANT)
            if (!doctor.IsVerified)
                throw new Exception("Doctor not verified by medical council");

            // 4️⃣ Save consultation details
            app.Diagnosis = d.Diagnosis;
            app.Prescription = d.Prescription;
            app.Fees = d.Fees;
            app.Status = "Consulted";

            await _apps.SaveAsync();
        }



        public async Task<List<AdminAppointmentDto>> GetAdminAppointments(
    int hospitalId,
    DateOnly date,
    string? status)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            var query = _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a =>
                    a.HospitalId == hospitalId &&
                    a.AppointmentDate == utcDate
                );

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            return await query
                .OrderBy(a => a.QueueToken)
                .Select(a => new AdminAppointmentDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.Doctor.User.Name,
                    a.Doctor.Specialization,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status,
                    a.QueueToken,
                    a.Fees,
                    a.IsPaid,
                    a.ReasonForVisit        // 👈
                ))
                .ToListAsync();
        }



        public async Task<List<AdminAppointmentDto>> GetAdminQueue(
    int hospitalId,
    DateOnly date)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            return await _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Where(a =>
                    a.HospitalId == hospitalId &&
                    a.AppointmentDate == utcDate &&
                    a.Status == "CheckedIn")
                .OrderBy(a => a.QueueToken)
                .Select(a => new AdminAppointmentDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.Doctor.User.Name,
                    a.Doctor.Specialization,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status,
                    a.QueueToken,
                    a.Fees,
                    a.IsPaid,
                    a.ReasonForVisit        // 👈
                ))
                .ToListAsync();
        }




        // =========================
        // PAYMENT DETAILS (ADMIN)
        // =========================
        public async Task<PaymentViewDto> GetPaymentDetails(int appointmentId)
        {
            var app = await _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.FamilyMember)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (app == null)
                throw new Exception("Appointment not found");

            return new PaymentViewDto(
                app.Id,
                app.Patient.User.Name,
                app.FamilyMember != null ? app.FamilyMember.Name : null,
                app.Doctor.User.Name,
                app.TimeSlot,
                app.Fees,
                app.IsPaid,
                app.Status
            );
        }


        // =========================
        // CONFIRM PAYMENT (ADMIN)
        // =========================


        public async Task ConfirmPayment(ConfirmPaymentDto dto)
        {
            var app = await _apps.GetAsync(x => x.Id == dto.AppointmentId);
            if (app == null)
                throw new Exception("Appointment not found");

            if (app.IsPaid)
                throw new Exception("Already paid");

            app.IsPaid = true;
            app.Status = "Completed";

            await _apps.SaveAsync();
        }



      


        // =========================
        // NEARBY DOCTORS
        // =========================
        public async Task<List<object>> GetNearbyDoctors(
            double lat, double lon, double radiusKm)
        {
            var doctors = await _d.Query()
                .Include(x => x.User)
                .ToListAsync();

            return doctors
                .Select(d => new
                {
                    d.Id,
                    d.User.Name,
                    d.Specialization,
                    Distance =
                        Math.Sqrt(
                            Math.Pow(lat - (d.User.Latitude ?? 0), 2) +
                            Math.Pow(lon - (d.User.Longitude ?? 0), 2)
                        ) * 111
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Cast<object>()
                .ToList();
        }





        public List<object> GetDoctorsForAdmin(int hospitalId)
        {
            return _d.Query()
                .Where(d => d.HospitalId == hospitalId)
                .Include(d => d.User)
                .Select(d => new
                {
                    d.Id,
                    d.User.Name,
                    d.User.MobileNumber,
                    d.Specialization
                })
                .Cast<object>()
                .ToList();
        }


        // =========================
        // ADMIN – VIEW APPOINTMENTS
        // =========================
        public List<object> GetAppointmentsForAdmin()
        {
            return _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Select(a => new
                {
                    a.Id,
                    Patient = a.Patient.User.Name,
                    Doctor = a.Doctor.User.Name,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status,
                    a.Fees,
                    a.IsPaid
                })
                .Cast<object>()
                .ToList();
        }


        public async Task SetupHospitalWithAdmin(HospitalSetupDto dto)
        {
            // 1️⃣ Create Hospital
            var hospital = new Hospital
            {
                Name = dto.HospitalName,
                Address = dto.Address,
                Phone = dto.Phone
            };

            await _hospital.AddAsync(hospital);
            await _hospital.SaveAsync();

            // 2️⃣ Create Admin User
            var user = new User
            {
                Name = dto.AdminName,
                MobileNumber = dto.AdminMobile,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword),
                Role = "Admin"
            };

            await _u.AddAsync(user);
            await _u.SaveAsync();

            // 3️⃣ Link Admin to Hospital
            await _admin.AddAsync(new Admin
            {
                UserId = user.Id,
                HospitalId = hospital.Id
            });

            await _admin.SaveAsync();
        }






        // =========================
        // JWT
        // =========================
       


        private string GenerateJwt(User u)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            // 🔑 Base claims (ALL users)
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
        new Claim(ClaimTypes.Role, u.Role)
    };

            // 🔒 HospitalId ONLY for Admin & Doctor
            if (u.Role == "Admin")
            {
                if (u.Admin == null)
                    throw new Exception("Admin not linked to hospital");

                claims.Add(
                    new Claim("HospitalId", u.Admin.HospitalId.ToString())
                );
            }
            else if (u.Role == "Doctor")
            {
                if (u.Doctor == null)
                    throw new Exception("Doctor not linked to hospital");

                claims.Add(
                    new Claim("HospitalId", u.Doctor.HospitalId.ToString())
                );
            }

            else if (u.Role == "Staff")
            {
                // 🔥 STAFF → DoctorId only
                var staff = _doctorStaff
                    .GetAsync(x => x.UserId == u.Id)
                    .GetAwaiter()
                    .GetResult();

                if (staff == null)
                    throw new Exception("Staff not linked to doctor");

                claims.Add(
                    new Claim("DoctorId", staff.DoctorId.ToString())
                );
            }

            else if (u.Role == "Patient")
            {
                // ✅ Patient-க்கு HospitalId claim வேண்டாம்
                // Nothing to add here
            }


            else
            {
                throw new Exception("Invalid role");
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials:
                    new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public async Task<PatientProfileDto> GetPatientProfile(int userId)
        {
            var patient = await _p.Query()
                .Include(p => p.User)
                .Include(p => p.FamilyMembers)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                throw new Exception("Patient not found");

            return new PatientProfileDto(
                patient.User.Name,
                patient.User.MobileNumber,
                patient.User.Latitude,
                patient.User.Longitude,
                patient.FamilyMembers
                    .Select(f => new FamilyMemberDto(
                        f.Id,
                        f.Name,
                        f.Relationship
                    )).ToList()
            );
        }




        public async Task UpdatePatientProfile(
    int userId,
    UpdatePatientProfileDto dto)
        {
            var patient = await _p.Query()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                throw new Exception("Patient not found");

            patient.User.Name = dto.Name;
            patient.User.Latitude = dto.Latitude;
            patient.User.Longitude = dto.Longitude;

            await _u.SaveAsync();
        }



        public async Task<List<PatientAppointmentDto>> GetPatientAppointments(
    int userId,
    string type) // upcoming | past
        {
            var patient = await _p.GetAsync(x => x.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            var today = DateTime.UtcNow.Date;

            var query = _apps.Query()
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Hospital)
                .Where(a => a.PatientId == patient.Id);

            if (type == "upcoming")
            {
                query = query.Where(a => a.AppointmentDate.Date >= today);
            }
            else if (type == "past")
            {
                query = query.Where(a => a.AppointmentDate.Date < today);
            }

            return await query
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new PatientAppointmentDto(
                    a.Id,
                    a.Doctor.User.Name,
                    a.Doctor.Specialization,
                    a.Doctor.Hospital.Name,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status
                ))
                .ToListAsync();
        }



        public async Task<List<HospitalListDto>> GetAllHospitals()
        {
            return await _hospital.Query()
                .Select(h => new HospitalListDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Phone
                ))
                .ToListAsync();
        }


        public async Task<List<DoctorPublicDto>> GetDoctorsByHospital(int hospitalId)
        {
            return await _d.Query()
                .Include(d => d.User)
                .Include(d => d.Hospital)
                .Where(d => d.HospitalId == hospitalId)
                .Select(d => new DoctorPublicDto(
                    d.Id,
                    d.User.Name,
                    d.Specialization,
                    d.Hospital.Name
                ))
                .ToListAsync();
        }


        public async Task<List<SpecialityDto>> GetSpecialities()
        {
            return await _d.Query()
                .Select(d => d.Specialization)
                .Distinct()
                .Select(s => new SpecialityDto(s))
                .ToListAsync();
        }










        private static DateTime ToUtcDate(DateOnly date)
        {
            return DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );
        }

    }
}
