using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HospitalProject.Helper;
using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        private readonly IRepository<MedicalRep> _medicalRep;
        private readonly IRepository<MedicalRepSlot> _medicalRepSlots;
        private readonly IRepository<MedicalRepAppointment> _medicalRepApps;




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
    // 🔥 ADD THESE 3
            IRepository<MedicalRep> medicalRep,
            IRepository<MedicalRepSlot> medicalRepSlots,
            IRepository<MedicalRepAppointment> medicalRepApps,
            ITwilioService twilio,
            IConfiguration config)
        {
            _u = u; _p = p; _d = d; _admin = admin;
            _slots = slots; _apps = apps; _otp = otp; _family = family; _hospital = hospital;
            _doctorProfile = doctorProfile;        // 👈
            _doctorDocument = doctorDocument;                                          
            _medicalRep = medicalRep;
            _medicalRepSlots = medicalRepSlots;
            _medicalRepApps = medicalRepApps;
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






        public async Task RegisterHospitalDoctor(HospitalDoctorRegDto d)
        {
            if (d.HospitalId <= 0)
                throw new Exception("Invalid hospital");

            var hospital = await _hospital.GetAsync(h => h.Id == d.HospitalId);
            if (hospital == null)
                throw new Exception("Hospital not found");

            if (await _u.GetAsync(x => x.MobileNumber == d.Mobile) != null)
                throw new Exception("Mobile already exists");

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

            await _d.AddAsync(new Doctor
            {
                UserId = user.Id,
                Specialization = d.Specialization,
                HospitalId = d.HospitalId,
                Latitude = d.Lat,
                Longitude = d.Lon,
                IsVerified = false
            });

            await _d.SaveAsync();
        }



        public async Task RegisterIndependentDoctor(
    IndependentDoctorRegDto d)
        {
            if (await _u.GetAsync(x => x.MobileNumber == d.Mobile) != null)
                throw new Exception("Mobile already exists");

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

            await _d.AddAsync(new Doctor
            {
                UserId = user.Id,
                Specialization = d.Specialization,
                HospitalId = null,      // 🔥 NO hospital
                Latitude = d.Lat,
                Longitude = d.Lon,
                IsVerified = false
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
        //public async Task<string> Login(LoginDto d)
        //{
        //    //var user = await _u.GetAsync(x => x.MobileNumber == d.MobileNumber);
        //    //        var user = await _u.Query()
        //    //.Include(x => x.Admin)
        //    //.Include(x => x.Doctor)
        //    //.Include(x => x.Patient)
        //    //.FirstOrDefaultAsync(x => x.MobileNumber == d.MobileNumber);

        //    var user = await _u.Query()
        //      .Include(x => x.Admin)
        //      .Include(x => x.Doctor)
        //      .Include(x => x.Patient)
        //      .FirstOrDefaultAsync(x =>
        //      x.MobileNumber == d.MobileNumber &&
        //      x.IsDeleted == false   // 🔥 ADD THIS
        //      );

        //    if (user == null || !BCrypt.Net.BCrypt.Verify(d.Password, user.Password))
        //        return "Invalid Credentials";

        //    var otp = new Random().Next(1000, 9999).ToString();

        //    await _otp.AddAsync(new OtpStore
        //    {
        //        UserId = user.Id,
        //        MobileNumber = user.MobileNumber,
        //        OtpCode = otp,
        //        OtpType = "SMS",
        //        Purpose = "LOGIN",
        //        CreatedTime = DateTime.UtcNow,
        //        Expiry = DateTime.UtcNow.AddMinutes(5),
        //        IsUsed = false,
        //        IsSent = true
        //    });

        //    await _otp.SaveAsync();

        //    await _twilio.SendOtpAsync(d.MobileNumber, otp);
        //    return "OTP Sent";
        //}

        public async Task<bool> Login(LoginDto d)
        {
            var user = await _u.Query()
                .Include(x => x.Admin)
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .FirstOrDefaultAsync(x =>
                    x.MobileNumber == d.MobileNumber &&
                    x.IsDeleted == false
                );

            if (user == null || !BCrypt.Net.BCrypt.Verify(d.Password, user.Password))
                throw new Exception("Invalid credentials");

            var otp = new Random().Next(1000, 9999).ToString();

            await _otp.AddAsync(new OtpStore
            {
                UserId = user.Id,
                MobileNumber = user.MobileNumber,
                OtpCode = otp,
                OtpType = "SMS",
                Purpose = "LOGIN",
                CreatedTime = DateTime.UtcNow,
                Expiry = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsSent = true
            });

            await _otp.SaveAsync();
            await _twilio.SendOtpAsync(d.MobileNumber, otp);

            return true; // 🔥 IMPORTANT
        }


        ////flow2 doctor create admin without password

        //    public async Task CreateIndependentDoctorAdmin(
        //int doctorUserId,
        //DoctorAdminCreateDto dto)
        //    {
        //        // 1️⃣ Doctor (logged-in user)
        //        var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
        //        if (doctor == null)
        //            throw new Exception("Doctor not found");

        //        // 2️⃣ Duplicate mobile check
        //        if (await _u.GetAsync(x => x.MobileNumber == dto.Mobile) != null)
        //            throw new Exception("Mobile already exists");

        //        // 3️⃣ Create ADMIN user
        //        var adminUser = new User
        //        {
        //            Name = dto.Name.Trim(),
        //            MobileNumber = dto.Mobile.Trim(),
        //            Password = BCrypt.Net.BCrypt.HashPassword("1234"), // temp
        //            Role = "Admin"
        //        };

        //        await _u.AddAsync(adminUser);
        //        await _u.SaveAsync();

        //        // 4️⃣ Link admin to doctor (NO hospital)
        //        await _doctorStaff.AddAsync(new DoctorStaff
        //        {
        //            DoctorId = doctor.Id,
        //            UserId = adminUser.Id,
        //            StaffRole = "Admin"
        //        });

        //        await _doctorStaff.SaveAsync();

        //        // 5️⃣ OTP for first login
        //        var otp = new Random().Next(1000, 9999).ToString();

        //        await _otp.AddAsync(new OtpStore
        //        {
        //            MobileNumber = dto.Mobile,
        //            OtpCode = otp,
        //            Expiry = DateTime.UtcNow.AddMinutes(5)
        //        });

        //        await _otp.SaveAsync();

        //        await _twilio.SendOtpAsync(
        //            dto.Mobile,
        //            $"You are added as clinic admin. OTP: {otp}"
        //        );
        //    }


        //flow2 doctor create admin with password
        public async Task CreateIndependentDoctorAdmin(
    int doctorUserId,
    DoctorAdminCreateDto dto)
        {
            // 1️⃣ Doctor (logged-in user)
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Duplicate mobile check
            if (await _u.GetAsync(x => x.MobileNumber == dto.Mobile) != null)
                throw new Exception("Mobile already exists");

            // 🔥 Generate TEMP PASSWORD
            var tempPassword = new Random().Next(1000, 9999).ToString();

            // 3️⃣ Create ADMIN user
            var adminUser = new User
            {
                Name = dto.Name.Trim(),
                MobileNumber = dto.Mobile.Trim(),
                Password = BCrypt.Net.BCrypt.HashPassword(tempPassword), // 🔥 temp password
                Role = "Admin"
            };

            await _u.AddAsync(adminUser);
            await _u.SaveAsync();

            // 4️⃣ Link admin to doctor (NO hospital)
            await _doctorStaff.AddAsync(new DoctorStaff
            {
                DoctorId = doctor.Id,
                UserId = adminUser.Id,
                StaffRole = "Admin"
            });

            await _doctorStaff.SaveAsync();

            // 5️⃣ OTP for first login
            var otp = new Random().Next(1000, 9999).ToString();

            await _otp.AddAsync(new OtpStore
            {
                UserId = adminUser.Id,
                MobileNumber = dto.Mobile,
                OtpCode = otp,
                OtpType = "SMS",
                Purpose = "LOGIN",
                CreatedTime = DateTime.UtcNow,
                Expiry = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsSent = true
            });

            await _otp.SaveAsync();

            // 6️⃣ SEND SMS (Temp Password + OTP)
            await _twilio.SendOtpAsync(
                dto.Mobile,
                $"You are added as Clinic Admin.\n" +
                $"Temp Password: {tempPassword}\n" +
                $"OTP: {otp}\n" +
                $"Please login and change your password."
            );
        }






        //----------------------
        // Verify Otp
        //----------------------

        //public async Task<(string Token, string Role, string Name, int UserId)> VerifyOtp(VerifyOtpDto d)
        //{
        //    var rec = await _otp.GetAsync(x =>
        //        x.MobileNumber == d.MobileNumber &&
        //        x.OtpCode == d.Otp &&
        //        x.Purpose == "LOGIN" &&
        //        x.IsUsed == false &&
        //        x.Expiry > DateTime.UtcNow
        //    );

        //    if (rec == null)
        //        throw new Exception("Invalid or expired OTP");

        //    rec.IsUsed = true;
        //    await _otp.SaveAsync();

        //    var user = await _u.Query()
        //        .Include(x => x.Admin)
        //        .Include(x => x.Doctor)
        //        .Include(x => x.Patient)
        //        .FirstOrDefaultAsync(x => x.Id == rec.UserId);

        //    if (user == null)
        //        throw new Exception("User not found");

        //    // 🔥 ADD THIS CHECK
        //    if (user.IsDeleted)
        //        throw new Exception("User account is deactivated");

        //    var token = GenerateJwt(user);

        //    return (token, user.Role, user.Name, user.Id);
        //}


        public async Task<(
    string Token,
    string Role,
    string Name,
    int UserId,
    int? AdminId,
    int? DoctorId,
    int? PatientId,
    int? MedicalRepId
)> VerifyOtp(VerifyOtpDto d)
        {
            var rec = await _otp.GetAsync(x =>
                x.MobileNumber == d.MobileNumber &&
                x.OtpCode == d.Otp &&
                x.Purpose == "LOGIN" &&
                x.IsUsed == false &&
                x.Expiry > DateTime.UtcNow
            );

            if (rec == null)
                throw new Exception("Invalid or expired OTP");

            rec.IsUsed = true;
            await _otp.SaveAsync();

            var user = await _u.Query()
                .Include(x => x.Admin)
                .Include(x => x.Doctor)
                .Include(x => x.Patient)
                .Include(x => x.MedicalRep)   // 🔥 ADD THIS
                .FirstOrDefaultAsync(x => x.Id == rec.UserId);

            if (user == null)
                throw new Exception("User not found");

            if (user.IsDeleted)
                throw new Exception("User account is deactivated");

            var token = GenerateJwt(user);

            return (
                token,
                user.Role,
                user.Name,
                user.Id,
                user.Admin?.Id,     // 👈 AdminId
                user.Doctor?.Id,    // 👈 DoctorId
                user.Patient?.Id,    // 👈 PatientId
                user.MedicalRep?.Id   // 🔥 THIS IS WHAT YOU WANT
            );
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
                UserId = user.Id,
                MobileNumber = user.MobileNumber,
                OtpCode = otp,
                OtpType = "SMS",
                Purpose = "ForgotPassword",
                CreatedTime = DateTime.UtcNow,
                Expiry = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsSent = true
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
                    x.AvailableDate == utcDate &&
                    x.IsClosed == false)
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
        // SELF BOOKING (Doctor based)
        // =========================
        public async Task<string> BookPatientByTime(
            int userId,
            PatientTimeBookingDto dto)
        {
            // 1️⃣ Patient check
            var patient = await _p.GetAsync(x => x.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            // 2️⃣ Doctor check (ONLY verified doctor)


            //var doctor = await _d.GetAsync(x =>
            //    x.Id == dto.DoctorId &&
            //    x.IsVerified == true);

            var doctor = await _d.Query()
                 .Include(d => d.User)
                 .FirstOrDefaultAsync(x =>
                 x.Id == dto.DoctorId &&
                 x.IsVerified == true);


            if (doctor == null)
                throw new Exception("Doctor not available");

            // 3️⃣ Convert date to UTC
            var utcDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            // 4️⃣ Slot availability check
            var slotExists = await _slots.GetAsync(x =>
                x.DoctorId == dto.DoctorId &&
                x.AvailableDate == utcDate &&
                x.TimeSlot == dto.TimeSlot &&
                x.IsClosed == false);   // ⭐ ADD THIS

            if (slotExists == null)
                throw new Exception("Selected time not available");

            // 5️⃣ Generate temp token
            var token = Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpper();

            // 6️⃣ Create appointment
            await _apps.AddAsync(new Appointment
            {
                HospitalId = doctor.HospitalId, // ✅ NULL or value – both OK
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                AppointmentDate = utcDate,
                TimeSlot = dto.TimeSlot.Trim(),
                TempToken = token,
                Status = "Booked",
                ReasonForVisit = dto.ReasonForVisit
            });

            await _apps.SaveAsync();

            // =========================
            // 🔔 SEND SMS TO PATIENT
            // =========================

            var patientUser = await _u.GetAsync(u => u.Id == userId);

            if (patientUser != null)
            {
                await _twilio.SendOtpAsync(
                    patientUser.MobileNumber,
                    $"Your appointment is booked.\n" +
                    $"Doctor: {doctor.User.Name}\n" +
                    $"Date: {dto.Date}\n" +
                    $"Time: {dto.TimeSlot}\n" +
                    $"Token: {token}\n" +
                    $"Please check-in on arrival."
                );
            }

            return token;
        }
            


        // =========================
        // FAMILY BOOKING (Doctor based)
        // =========================
        public async Task<string> BookFamilyByTime(
            int userId,
            FamilyTimeBookingDto dto)
        {
            // 1️⃣ Logged-in patient check
            var patient = await _p.GetAsync(x => x.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            // 2️⃣ Family member belongs to patient
            var family = await _family.GetAsync(x =>
                x.Id == dto.FamilyMemberId &&
                x.PatientId == patient.Id);

            if (family == null)
                throw new Exception("Invalid family member");

            // 3️⃣ Doctor check (ONLY verified)
            //var doctor = await _d.GetAsync(x =>
            //    x.Id == dto.DoctorId &&
            //    x.IsVerified == true);

            var doctor = await _d.Query()
           .Include(d => d.User)          // 👈 IMPORTANT
           .FirstOrDefaultAsync(x =>
            x.Id == dto.DoctorId &&
            x.IsVerified == true);


            if (doctor == null)
                throw new Exception("Doctor not available");

            // 4️⃣ Convert date to UTC
            var utcDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            // 5️⃣ Slot availability check
            var slotExists = await _slots.GetAsync(x =>
            x.DoctorId == dto.DoctorId &&
            x.AvailableDate == utcDate &&
            x.TimeSlot == dto.TimeSlot &&
            x.IsClosed == false   // ⭐ ADD THIS
            );

            if (slotExists == null)
                throw new Exception("Selected time slot not available");

            // 6️⃣ Generate temp token
            var tempToken = Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpper();

            // 7️⃣ Create appointment
            await _apps.AddAsync(new Appointment
            {
                HospitalId = doctor.HospitalId, // ✅ optional
                PatientId = patient.Id,
                FamilyMemberId = family.Id,
                DoctorId = doctor.Id,
                AppointmentDate = utcDate,
                TimeSlot = dto.TimeSlot.Trim(),
                TempToken = tempToken,
                Status = "Booked",
                ReasonForVisit = dto.ReasonForVisit
            });

            await _apps.SaveAsync();


            // =========================
            // 🔔 SEND SMS TO PATIENT
            // =========================

            var patientUser = await _u.GetAsync(u => u.Id == userId);

            if (patientUser != null)
            {
                await _twilio.SendOtpAsync(
                    patientUser.MobileNumber,
                    $"Appointment booked for {family.Name}.\n" +
                    $"Doctor: {doctor.User.Name}\n" +
                    $"Date: {dto.Date}\n" +
                    $"Time: {dto.TimeSlot}\n" +
                    $"Token: {tempToken}\n" +
                    $"Please check-in on arrival."
                );
            }

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
        //public async Task<int> CheckIn(string tempToken)
        //{
        //    var app = await _apps.GetAsync(x => x.TempToken == tempToken);
        //    if (app == null)
        //        throw new Exception("Invalid token");

        //    if (app.Status == "CheckedIn")
        //        return app.QueueToken!.Value;

        //    int count = _apps.Query().Count(x =>
        //        x.DoctorId == app.DoctorId &&
        //        x.AppointmentDate.Date == app.AppointmentDate.Date &&
        //        x.Status == "CheckedIn");

        //    app.Status = "CheckedIn";
        //    app.QueueToken = count + 1;

        //    await _apps.SaveAsync();
        //    return app.QueueToken.Value;
        //}

        // Queue Token reset for morning afternoon evening night

        //public async Task<int> CheckIn(string tempToken)
        //{
        //    var app = await _apps.GetAsync(x => x.TempToken == tempToken);
        //    if (app == null)
        //        throw new Exception("Invalid token");

        //    if (app.Status == "CheckedIn")
        //        return app.QueueToken!.Value;

        //    int count = _apps.Query().Count(x =>
        //        x.DoctorId == app.DoctorId &&
        //        x.AppointmentDate.Date == app.AppointmentDate.Date &&
        //        x.TimeSlot == app.TimeSlot &&   // ✅ slot based reset
        //        x.Status == "CheckedIn");

        //    app.Status = "CheckedIn";
        //    app.QueueToken = count + 1;

        //    await _apps.SaveAsync();
        //    return app.QueueToken.Value;
        //}

        // Queue Token reset for morning afternoon evening night


        public async Task<int> CheckIn(string tempToken)
        {
            // 1️⃣ Appointment fetch by temp token
            var app = await _apps.GetAsync(x => x.TempToken == tempToken);
            if (app == null)
                throw new Exception("Invalid token");

            // ❌ NoShow cannot check-in
            if (app.Status == "NoShow")
                throw new Exception("Appointment marked as NoShow. Check-in not allowed");

            // 2️⃣ Already checked-in → return existing queue token
            if (app.Status == "CheckedIn")
                return app.QueueToken!.Value;

            // 3️⃣ Appointment date validation (today only)
            if (app.AppointmentDate.Date != DateTime.UtcNow.Date)
                throw new Exception("Check-in allowed only on appointment date");

            // 4️⃣ Time slot validation (MOST IMPORTANT)
            if (!TimeSlotHelper.IsNowWithinSlot(app.TimeSlot))
                throw new Exception("Check-in not allowed for this time slot");

            // 5️⃣ Slot-based queue count (reset per slot)
            int count = _apps.Query().Count(x =>
                x.DoctorId == app.DoctorId &&
                x.AppointmentDate.Date == app.AppointmentDate.Date &&
                x.TimeSlot == app.TimeSlot &&
               (x.Status == "CheckedIn" || x.Status == "Completed"));

            // 6️⃣ Update appointment
            app.Status = "CheckedIn";
            app.QueueToken = count + 1;

            await _apps.SaveAsync();

            // 7️⃣ Return queue token
            return app.QueueToken.Value;
        }






        //Update vitals by admin
        //public async Task UpdateVitalsByAdmin(
        //int adminUserId,
        //UpdateVitalsDto dto)
        //{
        //    // 1️⃣ Admin validate
        //    var admin = await _u.GetAsync(x =>
        //        x.Id == adminUserId &&
        //        x.Role == "Admin" &&
        //        x.IsDeleted == false);

        //    if (admin == null)
        //        throw new Exception("Admin not found");

        //    // 2️⃣ Appointment fetch
        //    var app = await _apps.GetAsync(a => a.Id == dto.AppointmentId);
        //    if (app == null)
        //        throw new Exception("Appointment not found");

        //    // 3️⃣ Update vitals
        //    app.BloodPressure = dto.BloodPressure;
        //    app.Pulse = dto.Pulse;
        //    app.Temperature = dto.Temperature;
        //    app.SpO2 = dto.SpO2;

        //    await _apps.SaveAsync();
        //}




        // =========================
        // UPDATE VITALS BY ADMIN
        // =========================

        public async Task UpdateVitalsByAdmin(
    int adminUserId,
    UpdateVitalsDto dto)
        {
            // 1️⃣ Admin validate
            var admin = await _u.GetAsync(x =>
                x.Id == adminUserId &&
                x.Role == "Admin" &&
                x.IsDeleted == false);

            if (admin == null)
                throw new Exception("Admin not found");

            // 2️⃣ Appointment fetch
            var app = await _apps.GetAsync(a => a.Id == dto.AppointmentId);
            if (app == null)
                throw new Exception("Appointment not found");

            // ✅ 3️⃣ CHECK-IN VALIDATION (IMPORTANT)
            if (app.Status != "CheckedIn")
                throw new Exception("Vitals can be updated only after patient check-in");

            // 4️⃣ Update vitals
            app.BloodPressure = dto.BloodPressure;
            app.Pulse = dto.Pulse;
            app.Temperature = dto.Temperature;
            app.SpO2 = dto.SpO2;

            await _apps.SaveAsync();
        }


        // =========================
        // UPDATE VITALS BY DOCTOR
        // =========================

        public async Task UpdateVitalsByDoctor(
        int doctorUserId,
        UpdateVitalsDto dto)
        {
            // 1️⃣ Doctor validate
            var doctor = await _d.Query()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d =>
                    d.UserId == doctorUserId &&
                    d.User.IsDeleted == false);

            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Appointment fetch (belongs to doctor)
            var app = await _apps.GetAsync(a =>
                a.Id == dto.AppointmentId &&
                a.DoctorId == doctor.Id);

            if (app == null)
                throw new Exception("Appointment not found");

            // 3️⃣ Status validation
            if (app.Status != "CheckedIn" && app.Status != "Consulted")
                throw new Exception("Vitals can be updated only during consultation");

            // 4️⃣ Update vitals (doctor can overwrite admin values)
            app.BloodPressure = dto.BloodPressure;
            app.Pulse = dto.Pulse;
            app.Temperature = dto.Temperature;
            app.SpO2 = dto.SpO2;

            await _apps.SaveAsync();
        }






        // =========================
        // Doctor View Patient Vitals (Appointment based)
        // =========================


        public async Task<PatientWorkspaceDto>
        GetPatientVitalsForDoctorByAppointment(
        int doctorUserId,
        int appointmentId)
        {
            // 1️⃣ Doctor validate
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Appointment fetch (belongs to doctor)
            var app = await _apps.Query()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    a.DoctorId == doctor.Id);

            if (app == null)
                throw new Exception("Appointment not found");

            // 3️⃣ Patient
            var patient = app.Patient;

            int age = patient.Dob.HasValue
                ? DateTime.Today.Year - patient.Dob.Value.Year
                : 0;

            // 4️⃣ Return vitals (admin / doctor updated)
            return new PatientWorkspaceDto(
                patient.User.Name,
                age,
                patient.Gender ?? "",
                app.ReasonForVisit,
                app.BloodPressure,
                app.Pulse,
                app.Temperature,
                app.SpO2
            );
        }


        // =========================
        // Public api for get user details
        // =========================

        public async Task<PublicUserDetailsDto>
         GetPublicUserDetails(int userId)
        {
            var user = await _u.Query()
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.IsDeleted == false);

            if (user == null)
                throw new Exception("User not found");

            int? age = null;
            string? gender = null;

            if (user.Role == "Patient" && user.Patient?.Dob != null)
            {
                age = DateTime.Today.Year - user.Patient.Dob.Value.Year;
                gender = user.Patient.Gender;
            }

            return new PublicUserDetailsDto(
                user.Id,
                user.Name,
                user.Role,
                age,
                gender
            );
        }










        // =========================
        // Doctor – Appointments (Upcoming / Past)
        // =========================


    //    public async Task<List<DoctorAppointmentDto>> GetDoctorAppointments(
    //int userId,
    //string type)
    //    {
    //        var doctor = await _d.GetAsync(d => d.UserId == userId);
    //        if (doctor == null)
    //            throw new Exception("Doctor not found");

    //        var today = DateTime.UtcNow.Date;

    //        var query = _apps.Query()
    //            .Include(a => a.Patient).ThenInclude(p => p.User)
    //            .Include(a => a.FamilyMember)
    //            .Where(a => a.DoctorId == doctor.Id);

    //        if (type == "upcoming")
    //            query = query.Where(a => a.AppointmentDate.Date >= today);
    //        else if (type == "past")
    //            query = query.Where(a => a.AppointmentDate.Date < today);

    //        return await query
    //            .OrderBy(a => a.AppointmentDate)
    //            .ThenBy(a => a.TimeSlot)
    //            .Select(a => new DoctorAppointmentDto(
    //                a.Id,
    //                a.Patient.User.Name,
    //                a.FamilyMember != null ? a.FamilyMember.Name : null,
    //                a.AppointmentDate,
    //                a.TimeSlot,
    //                a.Status,
    //                a.QueueToken,
    //                a.ReasonForVisit      // 👈 MUST ADD
    //            ))
    //            .ToListAsync();
    //    }


        //********************************
        //Doctor profile Add  Method
        //********************************

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



        //********************************
        //Doctor profile View get Method
        //********************************

        public async Task<DoctorProfileViewDto> GetDoctorProfile(int userId)
        {
            // 1️⃣ Doctor fetch with User
            var doctor = await _d.Query()
                .Include(d => d.User)
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Doctor profile fetch
            var profile = await _doctorProfile.GetAsync(
                p => p.DoctorId == doctor.Id);

            if (profile == null)
                throw new Exception("Doctor profile not created");

            return new DoctorProfileViewDto(
                doctor.Id,
                doctor.User.Name,
                doctor.Specialization,

                profile.Dob,
                profile.Experience,
                profile.Languages,

                profile.LicenseType,
                profile.LicenseNumber,
                profile.StateCouncil,

                profile.Degree,
                profile.University,
                profile.GraduationYear,

                profile.PracticeMode,
                profile.ConsultationFee
            );
        }


        //Put method logic for doctor profile

        public async Task UpdateDoctorProfile(
    int userId,
    DoctorProfileCreateDto dto)
        {
            // 1️⃣ Doctor fetch
            var doctor = await _d.Query()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Existing profile fetch
            var profile = await _doctorProfile.GetAsync(
                p => p.DoctorId == doctor.Id);

            if (profile == null)
                throw new Exception("Doctor profile not created yet");

            // 3️⃣ Update fields
            profile.Dob = dto.Dob;
            profile.Experience = dto.Experience;
            profile.Languages = dto.Languages;

            profile.LicenseType = dto.LicenseType;
            profile.LicenseNumber = dto.LicenseNumber;
            profile.StateCouncil = dto.StateCouncil;

            profile.Degree = dto.Degree;
            profile.University = dto.University;
            profile.GraduationYear = dto.GraduationYear;

            profile.PracticeMode = dto.PracticeMode;
            profile.ConsultationFee = dto.ConsultationFee;

            await _doctorProfile.SaveAsync();
        }






        // =========================
        // Doctor – Today Queue List (CheckedIn)
        // =========================



        //    public async Task<List<DoctorAppointmentDto>> GetDoctorQueue(
        //int userId,
        //DateOnly date)
        //    {
        //        var doctor = await _d.GetAsync(d => d.UserId == userId);
        //        if (doctor == null)
        //            throw new Exception("Doctor not found");

        //        var utcDate = DateTime.SpecifyKind(
        //            date.ToDateTime(TimeOnly.MinValue),
        //            DateTimeKind.Utc
        //        );

        //        return await _apps.Query()
        //            .Include(a => a.Patient).ThenInclude(p => p.User)
        //            .Include(a => a.FamilyMember)
        //            .Where(a =>
        //                a.DoctorId == doctor.Id &&
        //                a.AppointmentDate == utcDate &&
        //                a.Status == "CheckedIn")
        //            .OrderBy(a => a.QueueToken)
        //            .Select(a => new DoctorAppointmentDto(
        //                a.Id,
        //                a.Patient.User.Name,
        //                a.FamilyMember != null ? a.FamilyMember.Name : null,
        //                a.AppointmentDate,
        //                a.TimeSlot,
        //                a.Status,
        //                a.QueueToken,
        //                a.ReasonForVisit      // 👈 MUST ADD
        //            ))
        //            .ToListAsync();
        //    }

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





        //************************************************
        // Admin view appointments with date status slot 
        //************************************************

        public async Task<List<AdminAppointmentDto>> GetAdminAppointments(
    DateOnly? date,
    string? status,
    string? timeSlot)
        {
            IQueryable<Appointment> query = _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Include(a => a.Doctor).ThenInclude(d => d.User);

            // 🔹 Date filter (optional)
            if (date.HasValue)
            {
                var utcDate = DateTime.SpecifyKind(
                    date.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc
                );

                query = query.Where(a =>
                    a.AppointmentDate.Date == utcDate.Date);
            }

            // 🔹 Status filter (optional)
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a =>
                    a.Status.ToLower() == status.ToLower());
            }

            // 🔹 Slot filter (optional)
            if (!string.IsNullOrWhiteSpace(timeSlot))
            {
                query = query.Where(a =>
                    a.TimeSlot == timeSlot.Trim());
            }

            return await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.QueueToken)
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
                    a.ReasonForVisit
                ))
                .ToListAsync();
        }







        public async Task<List<AdminAppointmentDto>> GetAdminQueue(
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
                    a.AppointmentDate.Date == utcDate.Date &&   // ✅ FIX
                    a.Status == "CheckedIn"
                )
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
                    a.ReasonForVisit
                ))
                .ToListAsync();
        }




        public async Task<List<AdminAppointmentDto>> GetDoctorQueuee(
     int doctorUserId,
     DateOnly date)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            var doctor = await _d.Query()
                .Include(d => d.User)   // ✅ FIX
                .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            return await _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.AppointmentDate.Date == utcDate.Date &&
                    a.Status == "CheckedIn")
                .OrderBy(a => a.QueueToken)
                .Select(a => new AdminAppointmentDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    doctor.User.Name,              // ✅ now safe
                    doctor.Specialization,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status,
                    a.QueueToken,
                    a.Fees,
                    a.IsPaid,
                    a.ReasonForVisit
                ))
                .ToListAsync();
        }





        //************************************************
        // Doctor view appointments with date status slot 
        //************************************************


        // public async Task<List<DoctorAppointmentDto>> GetDoctorAppointmentss(
        //int doctorUserId,
        //DateOnly? date,
        //string? status,
        //string? timeSlot)
        // {
        //     // 🔹 Get doctor by logged-in user
        //     var doctor = await _d.Query()
        //         .Include(d => d.User)
        //         .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

        //     if (doctor == null)
        //         throw new Exception("Doctor not found");

        //     // 🔹 IMPORTANT: declare as IQueryable
        //     IQueryable<Appointment> query = _apps.Query()
        //         .Include(a => a.Patient).ThenInclude(p => p.User)
        //         .Include(a => a.FamilyMember)
        //         .Where(a => a.DoctorId == doctor.Id);

        //     // 🔹 Date filter (optional)
        //     if (date.HasValue)
        //     {
        //         var utcDate = DateTime.SpecifyKind(
        //             date.Value.ToDateTime(TimeOnly.MinValue),
        //             DateTimeKind.Utc
        //         );

        //         query = query.Where(a =>
        //             a.AppointmentDate.Date == utcDate.Date);
        //     }

        //     // 🔹 Status filter (optional)
        //     if (!string.IsNullOrWhiteSpace(status))
        //     {
        //         query = query.Where(a =>
        //             a.Status.ToLower() == status.ToLower());
        //     }

        //     // 🔹 Slot filter (optional)
        //     if (!string.IsNullOrWhiteSpace(timeSlot))
        //     {
        //         query = query.Where(a =>
        //             a.TimeSlot == timeSlot.Trim());
        //     }

        //     return await query
        //         .OrderBy(a => a.AppointmentDate)
        //         .ThenBy(a => a.QueueToken)
        //         .Select(a => new DoctorAppointmentDto(
        //             a.Id,
        //             a.Patient.User.Name,
        //             a.FamilyMember != null ? a.FamilyMember.Name : null,
        //             a.AppointmentDate,
        //             a.TimeSlot,
        //             a.Status,
        //             a.QueueToken,
        //             a.ReasonForVisit
        //         ))
        //         .ToListAsync();
        // }

        //************************************************
        // Doctor view appointments with date status slot 
        //************************************************
        public async Task<List<DoctorAppointmentDto>> GetDoctorAppointmentss(
    int doctorUserId,
    DateOnly? date,
    string? status,
    string? timeSlot)
        {
            // 1️⃣ Get doctor by logged-in user
            var doctor = await _d.Query()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Base query
            IQueryable<Appointment> query = _apps.Query()
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .Include(a => a.FamilyMember)
                .Where(a => a.DoctorId == doctor.Id);

            // 3️⃣ Date filter (optional)
            if (date.HasValue)
            {
                var utcDate = DateTime.SpecifyKind(
                    date.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc
                );

                query = query.Where(a =>
                    a.AppointmentDate.Date == utcDate.Date);
            }

            // 4️⃣ Status filter (optional)
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a =>
                    a.Status.ToLower() == status.ToLower());
            }

            // 5️⃣ Slot filter (optional)
            if (!string.IsNullOrWhiteSpace(timeSlot))
            {
                query = query.Where(a =>
                    a.TimeSlot == timeSlot.Trim());
            }

            // 6️⃣ Projection to DTO
            return await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.QueueToken)
                .Select(a => new DoctorAppointmentDto(
                    a.Id,
                    a.Patient.User.Name,
                    a.FamilyMember != null ? a.FamilyMember.Name : null,
                    a.AppointmentDate,
                    a.TimeSlot,
                    a.Status,
                    a.QueueToken,

                    // 🔹 Vitals
                    a.BloodPressure,
                    a.Pulse,
                    a.Temperature,
                    a.SpO2,

                    // 🔹 Doctor consultation
                    a.Diagnosis,
                    a.Prescription,
                    a.Fees,

                    a.ReasonForVisit
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
                throw new Exception("Payment already completed");

            app.IsPaid = true;
            app.PaymentMode = dto.PaymentMode;
            app.PaidAt = DateTime.UtcNow;
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
                .Include(d => d.User)
                .Where(d =>
                    d.IsVerified == true &&
                    d.Latitude != null &&
                    d.Longitude != null)
                .ToListAsync();

            return doctors
                .Select(d => new
                {
                    d.Id,
                    d.User.Name,
                    d.Specialization,
                    Distance =
                        Math.Sqrt(
                            Math.Pow(lat - d.Latitude!.Value, 2) +
                            Math.Pow(lon - d.Longitude!.Value, 2)
                        ) * 111
                })
                .Where(x => x.Distance <= radiusKm)
                .OrderBy(x => x.Distance)
                .Cast<object>()
                .ToList();
        }






        //public List<object> GetDoctorsForAdmin(int hospitalId)
        //{
        //    return _d.Query()
        //        .Where(d => d.HospitalId == hospitalId)
        //        .Include(d => d.User)
        //        .Select(d => new
        //        {
        //            d.Id,
        //            d.User.Name,
        //            d.User.MobileNumber,
        //            d.Specialization
        //        })
        //        .Cast<object>()
        //        .ToList();
        //}


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

        //Patient History for Doctor

        public async Task<List<DoctorPatientHistoryDto>>
  GetPatientHistoryForDoctor(int doctorUserId, int patientId)
        {
            // 1️⃣ Doctor check
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Patient exists check
            var patient = await _p.GetAsync(p => p.Id == patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            // 3️⃣ History (only this doctor)
            return await _apps.Query()
                .Where(a =>
                    a.DoctorId == doctor.Id &&
                    a.PatientId == patientId &&
                    a.Status == "Completed")
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new DoctorPatientHistoryDto(
                    a.AppointmentDate,
                    a.ReasonForVisit,
                    a.Diagnosis,
                    a.Prescription,
                    a.Fees
                ))
                .ToListAsync();
        }


        // Patient OWN History

        public async Task<List<PatientHistoryDto>>
GetPatientHistory(int userId)
        {
            // 1️⃣ Patient fetch
            var patient = await _p.GetAsync(p => p.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            // 2️⃣ History
            return await _apps.Query()
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Where(a =>
                    a.PatientId == patient.Id &&
                    a.Status == "Completed")
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new PatientHistoryDto(
                    a.AppointmentDate,
                    a.Doctor.User.Name,
                    a.ReasonForVisit,
                    a.Diagnosis,
                    a.Prescription,
                    a.Fees
                ))
                .ToListAsync();
        }


        //Update Patient Details

        //   public async Task UpdatePatientPersonalDetails(
        //int userId,
        //PatientPersonalDetailsDto dto)
        //   {
        //       var patient = await _p.GetAsync(p => p.UserId == userId);
        //       if (patient == null)
        //           throw new Exception("Patient not found");

        //       patient.Dob = dto.Dob;
        //       patient.Gender = dto.Gender;
        //       patient.BloodGroup = dto.BloodGroup;
        //       patient.Email = dto.Email;

        //       patient.Address = dto.Address;
        //       patient.EmergencyContact = dto.EmergencyContact;

        //       patient.HeightCm = dto.HeightCm;
        //       patient.WeightKg = dto.WeightKg;

        //       patient.MedicalHistory = dto.MedicalHistory;
        //       patient.Allergies = dto.Allergies;

        //       await _p.SaveAsync();
        //   }



        public async Task UpdatePatientPersonalDetails(
    int userId,
    UpdatePatientPersonalDetailsDto dto)
        {
            var user = await _u.GetAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("User not found");

            var patient = await _p.GetAsync(p => p.UserId == userId);
            if (patient == null)
                throw new Exception("Patient not found");

            // 🔹 Update user
            user.Name = dto.Name;
            user.MobileNumber = dto.Phone;

            // 🔹 Update patient
            patient.Dob = dto.Dob;
            patient.Gender = dto.Gender;
            patient.Address = dto.Address;
            patient.BloodGroup = dto.BloodGroup;
            patient.EmergencyContact = dto.EmergencyContact;
            patient.HeightCm = dto.HeightCm;
            patient.WeightKg = dto.WeightKg;

            patient.MedicalHistory =
                JsonSerializer.Serialize(dto.MedicalHistory);

            // 🔥 DELETE OLD FAMILY (CORRECT WAY)
            var oldFamilyMembers = _family.Query()
                .Where(f => f.PatientId == patient.Id)
                .ToList();

            foreach (var member in oldFamilyMembers)
            {
                _family.Remove(member);
            }

            // 🔥 ADD NEW FAMILY
            foreach (var f in dto.Family)
            {
                await _family.AddAsync(new FamilyMember
                {
                    PatientId = patient.Id,
                    Name = f.Name,
                    Phone = f.Phone,
                    Dob = f.Dob,
                    Gender = f.Gender,
                    Relationship = f.Relation,
                    Address = f.Address,
                    BloodGroup = f.BloodGroup,
                    EmergencyContact = f.EmergencyContact,
                    HeightCm = f.HeightCm,
                    WeightKg = f.WeightKg
                });
            }

            await _u.SaveAsync();
            await _p.SaveAsync();
            await _family.SaveAsync();
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
                // Hospital Admin
                if (u.Admin?.HospitalId != null)
                {
                    claims.Add(
                        new Claim("HospitalId", u.Admin.HospitalId.ToString())
                    );
                }
                else
                {
                    // Doctor Clinic Admin
                    var staff = _doctorStaff
                        .GetAsync(x => x.UserId == u.Id)
                        .GetAwaiter()
                        .GetResult();

                    if (staff == null)
                        throw new Exception("Admin not linked to hospital or doctor");

                    claims.Add(
                        new Claim("DoctorId", staff.DoctorId.ToString())
                    );
                }
            }

            else if (u.Role == "Doctor")
            {
                if (u.Doctor?.HospitalId != null)
                {
                    claims.Add(
                        new Claim("HospitalId", u.Doctor.HospitalId.Value.ToString())
                    );
                }
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


            else if (u.Role == "MedicalRep")
            {
                // 🔥 MedicalRep – no extra claims needed
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



        //public async Task<PatientProfileDto> GetPatientProfile(int userId)
        //{
        //    var patient = await _p.Query()
        //        .Include(p => p.User)
        //        .Include(p => p.FamilyMembers)
        //        .FirstOrDefaultAsync(p => p.UserId == userId);

        //    if (patient == null)
        //        throw new Exception("Patient not found");

        //    return new PatientProfileDto(
        //        patient.User.Name,
        //        patient.User.MobileNumber,
        //        patient.User.Latitude,
        //        patient.User.Longitude,
        //        patient.FamilyMembers
        //            .Select(f => new FamilyMemberDto(
        //                f.Id,
        //                f.Name,
        //                f.Relationship
        //            )).ToList()
        //    );
        //}




    //    public async Task UpdatePatientProfile(
    //int userId,
    //UpdatePatientProfileDto dto)
    //    {
    //        var patient = await _p.Query()
    //            .Include(p => p.User)
    //            .FirstOrDefaultAsync(p => p.UserId == userId);

    //        if (patient == null)
    //            throw new Exception("Patient not found");

    //        patient.User.Name = dto.Name;
    //        patient.User.Latitude = dto.Latitude;
    //        patient.User.Longitude = dto.Longitude;

    //        await _u.SaveAsync();
    //    }



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
                .Include(a => a.FamilyMember)   // ⭐ ADD THIS
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
                     a.TempToken,
    a.Doctor.User.Name,
    a.Doctor.Specialization,
    a.Doctor.Hospital != null
        ? a.Doctor.Hospital.Name
        : "Independent Doctor",   // ✅ SAFE
    a.AppointmentDate,
    a.TimeSlot,
    a.Status,
    a.FamilyMember != null
        ? a.FamilyMember.Name
        : null
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
    d.Hospital != null ? d.Hospital.Name : "-"
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


        // =========================
        // PATIENT PERSONAL DETAILS DOCTOR VIEW
        // =========================


        public async Task<PatientBasicDetailsDto>
            GetPatientBasicDetailsForDoctor(int doctorUserId, int patientId)
        {
            // Doctor check
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // Patient + User fetch
            var patient = await _p.Query()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                throw new Exception("Patient not found");

            // (Optional but good) – doctor only sees his patient
            var hasAppointment = await _apps.GetAsync(a =>
                a.DoctorId == doctor.Id &&
                a.PatientId == patientId);

            if (hasAppointment == null)
                throw new Exception("Unauthorized patient access");

            return new PatientBasicDetailsDto(
                patient.User.Name,
                patient.Dob,
                patient.Gender,
                patient.BloodGroup,
                patient.Email
            );
        }

     // ---------------------------------
     // Doctor Arrived send message to patient
     // ---------------------------------
        public async Task MarkDoctorArrived(
    int adminUserId,
    int doctorId)
        {
            // 1️⃣ Admin validate
            var admin = await _u.GetAsync(x =>
                x.Id == adminUserId &&
                x.Role == "Admin" &&
                x.IsDeleted == false);

            if (admin == null)
                throw new Exception("Admin not found");

            // 2️⃣ Doctor
            var doctor = await _d.GetAsync(d => d.Id == doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 3️⃣ Mark arrived
            doctor.IsArrivedToday = true;
            doctor.ArrivedTime = DateTime.UtcNow;

            await _d.SaveAsync();

            // 4️⃣ Get today's booked appointments
            var todaysAppointments = await _apps.Query()
                .Include(a => a.Patient)
                .ThenInclude(p => p.User)
                .Where(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == DateTime.Today &&
                    a.Status == "Booked")
                .ToListAsync();

            // 5️⃣ Send SMS to patients
            foreach (var app in todaysAppointments)
            {
                var mobile = app.Patient.User.MobileNumber;

                await _twilio.SendOtpAsync(
                    mobile,
                    "Doctor has arrived at the clinic and consultations have started. Please be ready."
                );
            }
        }



        public async Task<PatientPersonalDetailsViewDto>
    GetPatientPersonalDetails(int userId)
        {
            var user = await _u.GetAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("User not found");

            var patient = await _p.Query()
                .Include(p => p.FamilyMembers)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                throw new Exception("Patient not found");

            // Medical history JSON → List<string>
            var medicalHistory = string.IsNullOrEmpty(patient.MedicalHistory)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(patient.MedicalHistory)
                    ?? new List<string>();

            return new PatientPersonalDetailsViewDto(
                user.Id,
                user.Name,
                user.MobileNumber,
                patient.Dob,
                patient.Gender,
                patient.Address,
                patient.BloodGroup,
                patient.EmergencyContact,
                patient.HeightCm,
                patient.WeightKg,
                medicalHistory,
                patient.FamilyMembers.Select(f =>
                    new FamilyMemberViewDto(
                        f.Id,
                        f.Name,
                        f.Phone,
                        f.Dob,
                        f.Gender,
                        f.Relationship,
                        f.Address,
                        f.BloodGroup,
                        f.EmergencyContact,
                        f.HeightCm,
                        f.WeightKg
                    )
                ).ToList()
            );
        }


        //**************************************
        // patient can view queue line details
        //***************************************


        public async Task<PatientQueueStatusDto>
        GetPatientQueueStatusByTempToken(string tempToken)
        {
            // 1️⃣ Get patient appointment
            var appointment = await _apps.GetAsync(a =>
                a.TempToken == tempToken);

            if (appointment == null)
                throw new Exception("Invalid token");

            if (appointment.QueueToken == null)
                throw new Exception("Patient not checked-in yet");

            // 2️⃣ Get all checked-in / consulted patients (same doctor + date + slot)
            var list = await _apps.Query()
                .Where(a =>
                    a.DoctorId == appointment.DoctorId &&
                    a.AppointmentDate == appointment.AppointmentDate &&
                    a.TimeSlot == appointment.TimeSlot &&
                    (a.Status == "CheckedIn" || a.Status == "Consulted"))
                .OrderBy(a => a.QueueToken)
                .ToListAsync();

            // 3️⃣ Find currently serving patient (latest Consulted)
            var currentServing = list
                .Where(a => a.Status == "Consulted")
                .OrderByDescending(a => a.QueueToken)
                .FirstOrDefault();

            // 4️⃣ Count patients before current patient
            int beforeYou = list.Count(a =>
                a.QueueToken < appointment.QueueToken &&
                a.Status == "CheckedIn");

            return new PatientQueueStatusDto(
                appointment.QueueToken.Value,
                currentServing?.QueueToken,
                beforeYou,
                list.Count
            );
        }




        //***************************************
        // Admin view doctor details for Add slot
        //***************************************

        public async Task<List<AdminDoctorViewDto>> GetDoctorsForAdmin()
        {
            return await _d.Query()
                .Include(d => d.User)
                .Where(d => d.User.IsDeleted == false)
                .OrderBy(d => d.User.Name)
                .Select(d => new AdminDoctorViewDto(
                    d.Id,
                    d.User.Name,
                    d.User.MobileNumber,
                    d.Specialization,
                    d.IsVerified,
                    !d.User.IsDeleted
                ))
                .ToListAsync();
        }

        //***************************************
        // No show status for time finish for booking
        //***************************************

        //public async Task MarkNoShowAppointments()
        //{
        //    var today = DateTime.UtcNow.Date;

        //    var appointments = await _apps.Query()
        //        .Where(a =>
        //            a.Status == "Booked" &&
        //            a.AppointmentDate.Date == today
        //        )
        //        .ToListAsync();

        //    foreach (var app in appointments)
        //    {
        //        // Slot time mudinjiducha?
        //        if (TimeSlotHelper.IsSlotOver(app.TimeSlot))
        //        {
        //            app.Status = "NoShow";
        //        }
        //    }

        //    await _apps.SaveAsync();
        //}



        public async Task<int> MarkNoShowBySlot(
    DateOnly date,
    string timeSlot)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            var appointments = await _apps.Query()
                .Where(a =>
                    a.AppointmentDate.Date == utcDate.Date &&
                    a.TimeSlot == timeSlot &&
                    a.Status == "Booked")   // 👈 IMPORTANT
                .ToListAsync();

            foreach (var app in appointments)
            {
                app.Status = "NoShow";
            }

            await _apps.SaveAsync();

            return appointments.Count;
        }


     //// automatic noshow 24 hours
     //   public async Task MarkAutoNoShowAppointments()
     //   {
     //       var today = DateTime.UtcNow.Date;

     //       var apps = await _apps.Query()
     //           .Where(a =>
     //               a.AppointmentDate.Date < today &&
     //               a.Status == "Booked")
     //           .ToListAsync();

     //       if (!apps.Any())
     //           return;

     //       foreach (var app in apps)
     //       {
     //           app.Status = "NoShow";
     //       }

     //       await _apps.SaveAsync();
     //   }





        public async Task BlockAdminByDoctor(
    int doctorUserId,
    int adminUserId)
        {
            // 1️⃣ Doctor validate
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Admin validate
            var adminUser = await _u.GetAsync(u =>
                u.Id == adminUserId &&
                u.Role == "Admin");

            if (adminUser == null)
                throw new Exception("Admin not found");

            // 3️⃣ Block admin
            adminUser.IsDeleted = true;
            await _u.SaveAsync();
        }

        public async Task UnblockAdminByDoctor(
            int doctorUserId,
            int adminUserId)
        {
            // 1️⃣ Doctor validate
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Admin validate
            var adminUser = await _u.GetAsync(u =>
                u.Id == adminUserId &&
                u.Role == "Admin");

            if (adminUser == null)
                throw new Exception("Admin not found");

            // 3️⃣ Unblock admin
            adminUser.IsDeleted = false;
            await _u.SaveAsync();
        }



        public async Task<List<AdminListForDoctorDto>> GetAdminsForDoctor(
         int doctorUserId)
        {
            // 1️⃣ Doctor validate
            var doctor = await _d.GetAsync(d => d.UserId == doctorUserId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            // 2️⃣ Get all admins
            return await _u.Query()
                .Where(u => u.Role == "Admin")
                .Select(u => new AdminListForDoctorDto(
                    u.Id,                 // 👈 THIS IS admin USER ID
                    u.Name,
                    u.MobileNumber,
                    u.IsDeleted            // true = blocked
                ))
                .ToListAsync();
        }

     

     //END SESION GET BY SLOT ID

        public async Task EndSessionBySlot(
        int userId,
        string role,
        int slotId)
        {
            // 1️⃣ Get slot
            var slot = await _slots.GetAsync(s => s.Id == slotId);
            if (slot == null)
                throw new Exception("Slot not found");

            IQueryable<Appointment> query = _apps.Query()
                .Where(a =>
                    a.AppointmentDate.Date == slot.AvailableDate.Date &&
                    a.TimeSlot == slot.TimeSlot &&
                    a.Status == "Booked");

            // 🔹 Doctor scope
            if (role == "Doctor")
            {
                var doctor = await _d.GetAsync(d => d.UserId == userId);
                if (doctor == null)
                    throw new Exception("Doctor not found");

                query = query.Where(a => a.DoctorId == doctor.Id);
            }

            // 🔹 Admin → all doctors allowed (or hospital filter if needed)

            var apps = await query.ToListAsync();

            foreach (var app in apps)
            {
                app.Status = "NoShow";
            }

            // 2️⃣ CLOSE SLOT
            slot.IsClosed = true;

            await _apps.SaveAsync();
            await _slots.SaveAsync();
        }



        public async Task RegisterMedicalRep(MedicalRepProfileDto dto)
        {
            if (await _u.GetAsync(x => x.MobileNumber == dto.Mobile) != null)
                throw new Exception("Mobile already exists");

            var user = new User
            {
                Name = dto.Name,
                MobileNumber = dto.Mobile,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "MedicalRep"
            };

            await _u.AddAsync(user);
            await _u.SaveAsync();

            await _medicalRep.AddAsync(new MedicalRep
            {
                UserId = user.Id,
                CompanyName = dto.CompanyName,
                Designation = dto.Designation,
                Area = dto.Area,
                IdProofNumber = dto.IdProofNumber
            });

            await _medicalRep.SaveAsync();
        }


        public async Task AddMedicalRepSlot(MedicalRepSlotCreateDto dto)
        {
            var utcDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            await _medicalRepSlots.AddAsync(new MedicalRepSlot
            {
                DoctorId = dto.DoctorId,
                SlotDate = utcDate,
                TimeSlot = dto.TimeSlot,

                MaxReps = dto.MaxReps,     // 🔥 eg: 20
                BookedCount = 0,           // 🔥 start
                IsClosed = false
            });

            await _medicalRepSlots.SaveAsync();
        }



        public async Task<List<MedicalRepSlot>> GetMedicalRepSlots(
    int doctorId,
    DateOnly date)
        {
            var utcDate = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            return await _medicalRepSlots.Query()
                .Where(x =>
                    x.DoctorId == doctorId &&
                    x.SlotDate == utcDate &&
                    x.IsClosed == false &&
                    x.BookedCount < x.MaxReps   // 🔥 KEY LOGIC
                )
                .ToListAsync();
        }


        public async Task<List<DoctorListDto>> GetDoctorsForMedicalRep()
        {
            return await _d.Query()
                .Include(d => d.User)
                .Include(d => d.Hospital)
                .Where(d => d.IsVerified == true)
                .Select(d => new DoctorListDto
                {
                    DoctorId = d.Id,
                    DoctorName = d.User.Name,
                    Specialization = d.Specialization,
                    HospitalName = d.Hospital != null
                        ? d.Hospital.Name
                        : ""
                })
                .ToListAsync();
        }




        public async Task<string> BookMedicalRepByTime(
     int userId,
     MedicalRepTimeBookingDto dto)
        {
            var rep = await _medicalRep.GetAsync(x => x.UserId == userId);
            if (rep == null)
                throw new Exception("Medical rep not found");

            var doctor = await _d.GetAsync(x =>
                x.Id == dto.DoctorId &&
                x.IsVerified == true);

            if (doctor == null)
                throw new Exception("Doctor not available");

            var utcDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc
            );

            var slot = await _medicalRepSlots.GetAsync(x =>
                x.DoctorId == dto.DoctorId &&
                x.SlotDate == utcDate &&
                x.TimeSlot == dto.TimeSlot &&
                x.IsClosed == false);

            if (slot == null)
                throw new Exception("Slot not available");

            if (slot.BookedCount >= slot.MaxReps)
                throw new Exception("Slot is full");

            // 🔥 Increase count
            slot.BookedCount += 1;
            await _medicalRepSlots.SaveAsync();

            var token = Guid.NewGuid().ToString("N")[..8].ToUpper();

            await _medicalRepApps.AddAsync(new MedicalRepAppointment
            {
                MedicalRepId = rep.Id,
                DoctorId = doctor.Id,
                AppointmentDate = utcDate,
                TimeSlot = dto.TimeSlot,
                TempToken = token,
                Status = "Booked"
            });

            await _medicalRepApps.SaveAsync();

            return token;
        }



        public async Task<int> CheckInMedicalRep(string tempToken)
        {
            var app = await _medicalRepApps.GetAsync(x =>
                x.TempToken == tempToken);

            if (app == null)
                throw new Exception("Invalid token");

            // ❌ NoShow / Completed cannot check-in again
            if (app.Status == "NoShow" || app.Status == "Completed")
                throw new Exception("Check-in not allowed for this appointment");


            if (app.Status == "CheckedIn")
                return app.QueueToken!.Value;


            // 3️⃣ Appointment date validation (TODAY only) ✅
            if (app.AppointmentDate.Date != DateTime.UtcNow.Date)
                throw new Exception("Check-in allowed only on appointment date");

            // 4️⃣ Time slot validation (MOST IMPORTANT) ✅
            if (!TimeSlotHelper.IsNowWithinSlot(app.TimeSlot))
                throw new Exception("Check-in not allowed for this time slot");

            // 5️⃣ Queue count (CheckedIn + Completed) ✅
            int count = _medicalRepApps.Query().Count(x =>
                x.DoctorId == app.DoctorId &&
                x.AppointmentDate.Date == app.AppointmentDate.Date &&
                x.TimeSlot == app.TimeSlot &&
                (x.Status == "CheckedIn" || x.Status == "Completed"));


            // 6️⃣ Update appointment
            app.Status = "CheckedIn";
            app.QueueToken = count + 1;

            await _medicalRepApps.SaveAsync();

            // 7️⃣ Return queue token
            return app.QueueToken.Value;
        }



        public async Task<List<MedicalRepBookingViewDto>>
 GetMedicalRepBookings(
     int userId,
     string role,
     int? doctorId,
     DateOnly? date,
     string? timeSlot)
        {
            // 🔹 Base query
            var query = _medicalRepApps.Query()
                .Include(a => a.MedicalRep)
                    .ThenInclude(r => r.User)
                .AsQueryable();

            // 🔹 Role based doctor filter
            if (role == "Doctor")
            {
                var doctor = await _d.GetAsync(d => d.UserId == userId);
                if (doctor == null)
                    throw new Exception("Doctor not found");

                query = query.Where(a => a.DoctorId == doctor.Id);
            }
            else if (role == "Admin" && doctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == doctorId.Value);
            }

            // 🔹 Date filter (optional)
            if (date.HasValue)
            {
                var utcDate = DateTime.SpecifyKind(
                    date.Value.ToDateTime(TimeOnly.MinValue),
                    DateTimeKind.Utc);

                query = query.Where(a => a.AppointmentDate == utcDate);
            }

            // 🔹 Slot filter (optional)
            if (!string.IsNullOrWhiteSpace(timeSlot))
            {
                query = query.Where(a => a.TimeSlot == timeSlot);
            }

            // 🔹 Final projection
            return await query
                .OrderBy(a => a.QueueToken)
                .Select(a => new MedicalRepBookingViewDto
                {
                    AppointmentId = a.Id,
                    MedicalRepName = a.MedicalRep.User.Name,
                    Mobile = a.MedicalRep.User.MobileNumber,
                    Status = a.Status,
                    TempToken = a.TempToken,
                    QueueToken = a.QueueToken
                })
                .ToListAsync();
        }



        public async Task ConsultMedicalRep(MedicalRepConsultDto dto)
        {
            var app = await _medicalRepApps.GetAsync(x =>
                x.Id == dto.AppointmentId);

            if (app == null)
                throw new Exception("Appointment not found");

            // 🔒 Only CheckedIn appointment can be completed
            if (app.Status != "CheckedIn")
                throw new Exception("Medical rep not checked in");

            // 📝 SAVE DOCTOR NOTES
            app.DoctorNotes = dto.DoctorNotes;

            // ✅ COMPLETE
            app.Status = "Completed";

            await _medicalRepApps.SaveAsync();
        }


        // ======================================
        // END MEDICAL REP SESSION BY SLOT
        // ======================================
        public async Task EndMedicalRepSessionBySlot(
     int userId,
     string role,
     int slotId)
        {
            // 1️⃣ Get slot
            var slot = await _medicalRepSlots.GetAsync(s => s.Id == slotId);
            if (slot == null)
                throw new Exception("Medical rep slot not found");

            // 2️⃣ Base query – ONLY BOOKED (🔥 FIX)
            IQueryable<MedicalRepAppointment> query =
                _medicalRepApps.Query()
                .Where(a =>
                    a.AppointmentDate.Date == slot.SlotDate.Date &&
                    a.TimeSlot == slot.TimeSlot &&
                    a.Status == "Booked");   // ✅ ONLY BOOKED

            // 3️⃣ Doctor scope
            if (role == "Doctor")
            {
                var doctor = await _d.GetAsync(d => d.UserId == userId);
                if (doctor == null)
                    throw new Exception("Doctor not found");

                query = query.Where(a => a.DoctorId == doctor.Id);
            }

            // 4️⃣ Mark ONLY booked reps as NoShow
            var apps = await query.ToListAsync();

            foreach (var app in apps)
            {
                app.Status = "NoShow";
            }

            // 5️⃣ Close slot
            slot.IsClosed = true;

            await _medicalRepApps.SaveAsync();
            await _medicalRepSlots.SaveAsync();
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
