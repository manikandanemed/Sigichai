using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HospitalProject.Models;
using HospitalProject.Services;
using System.Security.Claims;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/hospital")]
    public class HospitalController : ControllerBase
    {
        private readonly HospitalService _service;

        public HospitalController(HospitalService service)
        {
            _service = service;
        }

        // =========================
        // REGISTRATION APIs
        // =========================

        [HttpPost("register/patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRegDto dto)
        {
            await _service.RegisterPatient(dto);
            return Ok("Patient registered successfully");
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateHospital(
        HospitalCreateDto dto)
        {
            var hospitalId = await _service.CreateHospital(dto);

            return Ok(new
            {
                HospitalId = hospitalId,
                Message = "Hospital created successfully"
            });
        }






        //[HttpPost("hospital/doctor/register")]
        //public async Task<IActionResult> RegisterDoctor(DoctorRegDto dto)
        //{
        //    int hospitalId = int.Parse(
        //        User.FindFirst("HospitalId")!.Value
        //    );

        //    await _service.RegisterDoctor(hospitalId, dto);
        //    return Ok("Doctor registered successfully");
        //}


        [HttpPost("doctor/register/hospital")]
        public async Task<IActionResult> RegisterHospitalDoctor(
       HospitalDoctorRegDto dto)
        {
            await _service.RegisterHospitalDoctor(dto);
            return Ok("Hospital doctor registered. Pending verification");
        }


        [HttpPost("doctor/register/independent")]
        public async Task<IActionResult> RegisterIndependentDoctor(
    IndependentDoctorRegDto dto)
        {
            await _service.RegisterIndependentDoctor(dto);
            return Ok("Independent doctor registered. Pending verification");
        }



        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/admin/create")]
        public async Task<IActionResult> CreateDoctorAdmin(
    DoctorAdminCreateDto dto)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.CreateIndependentDoctorAdmin(
                doctorUserId, dto);

            return Ok("Admin created for your clinic & OTP sent");
        }



        //SUPER ADMIN

        [HttpPost("hospital")]
        public async Task<IActionResult> SetupHospital(
        HospitalSetupDto dto)
        {
            await _service.SetupHospitalWithAdmin(dto);
            return Ok("Hospital & First Admin created");
        }






        [Authorize(Roles = "Admin")]
        [HttpPost("hospital/admin/register")]
        public async Task<IActionResult> RegisterAdmin(AdminRegDto dto)
        {
            int hospitalId = int.Parse(
                User.FindFirst("HospitalId")!.Value
            );

            await _service.RegisterAdmin(hospitalId, dto);
            return Ok("Admin registered");
        }


        // =========================
        // LOGIN + OTP APIs
        // =========================

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _service.Login(dto);
            return Ok(result);
        }

        //[HttpPost("verify-otp")]
        //public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        //{
        //    var token = await _service.VerifyOtp(dto);
        //    return Ok(new { token });
        //}


        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var result = await _service.VerifyOtp(dto);

            return Ok(new
            {
                token = result.Token,
                role = result.Role
            });
        }


        // =========================
        // FORGOT / RESET PASSWORD
        // =========================

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto dto)
        {
            await _service.ForgotPassword(dto);
            return Ok("OTP sent to registered mobile number");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordDto dto)
        {
            await _service.ResetPassword(dto);
            return Ok("Password reset successfully");
        }


        // =========================
        // DOCTOR SEARCH & SLOTS
        // =========================

        [HttpGet("doctors/nearby")]
        public async Task<IActionResult> GetNearbyDoctors(
            double lat,
            double lon,
            double radiusKm = 10)
        {
            var doctors = await _service.GetNearbyDoctors(lat, lon, radiusKm);
            return Ok(doctors);
        }

        [HttpGet("doctor/{doctorId}/slots")]
        public async Task<IActionResult> GetSlots(
     int doctorId,
     DateOnly date)
        {
            var slots = await _service.GetAvailableSlots(doctorId, date);
            return Ok(slots);
        }





        // =========================
        // SELF BOOKING
        // =========================


        [Authorize(Roles = "Patient")]
        [HttpPost("appointment/book/self/by-time")]
        public async Task<IActionResult> BookSelfByTime(
    PatientTimeBookingDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var token = await _service.BookPatientByTime(userId, dto);
            return Ok(new { tempToken = token });
        }




        // =========================
        // PATIENT PROFILE
        // =========================

        [Authorize(Roles = "Patient")]
        [HttpGet("patient/profile")]
        public async Task<IActionResult> GetPatientProfile()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var profile = await _service.GetPatientProfile(userId);
            return Ok(profile);
        }

        [Authorize(Roles = "Patient")]
        [HttpPut("patient/profile")]
        public async Task<IActionResult> UpdatePatientProfile(
            UpdatePatientProfileDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.UpdatePatientProfile(userId, dto);
            return Ok("Profile updated successfully");
        }


        // =========================
        // PUBLIC APIs (NO AUTH)
        // =========================

        [HttpGet("public/hospitals")]
        public async Task<IActionResult> GetAllHospitals()
        {
            return Ok(await _service.GetAllHospitals());
        }

        [HttpGet("public/hospital/{hospitalId}/doctors")]
        public async Task<IActionResult> GetDoctorsByHospital(int hospitalId)
        {
            return Ok(await _service.GetDoctorsByHospital(hospitalId));
        }

        [HttpGet("public/specialities")]
        public async Task<IActionResult> GetSpecialities()
        {
            return Ok(await _service.GetSpecialities());
        }



        // =========================
        // PATIENT APPOINTMENTS
        // =========================

        [Authorize(Roles = "Patient")]
        [HttpGet("patient/appointments")]
        public async Task<IActionResult> GetPatientAppointments(
            [FromQuery] string type = "upcoming")
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var list = await _service.GetPatientAppointments(userId, type);
            return Ok(list);
        }




        // =========================
        // FAMILY APPOINTMENT
        // =========================


        [Authorize(Roles = "Patient")]
        [HttpPost("appointment/book/family/by-time")]
        public async Task<IActionResult> BookFamilyByTime(
    FamilyTimeBookingDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var token = await _service.BookFamilyByTime(userId, dto);
            return Ok(new { tempToken = token });
        }


        // =========================
        // ADD FAMILY
        // =========================


        [Authorize(Roles = "Patient")]
        [HttpPost("family")]
        public async Task<IActionResult> AddFamily(AddFamilyMemberDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _service.AddFamilyMember(userId, dto);
            return Ok("Family member added");
        }




        // =========================
        // ONLINE BOOKINGS LIST (BOOKED)
        // =========================



        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("appointments/booked")]
        public async Task<IActionResult> GetBookedAppointments(DateOnly date)
        {
            var list = await _service.GetOnlineBookingsByDate(date);
            return Ok(list);
        }



        // =========================
        //CHECK-IN LIST + QUEUE
        // ===========================


        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("appointments/checkedin")]
        public async Task<IActionResult> GetCheckedInAppointments(DateOnly date)
        {
            var list = await _service.GetCheckedInAppointmentsByDate(date);
            return Ok(list);
        }

        //***********************
        //Doctor profile create
        //************************


        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/profile")]
        public async Task<IActionResult> CreateDoctorProfile(
    DoctorProfileCreateDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.AddDoctorProfile(userId, dto);
            return Ok("Doctor profile created");
        }


        //***********************
        //Doctor profile Upload
        //************************

        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/document")]
        public async Task<IActionResult> UploadDoctorDocument(
    IFormFile file,
    string documentType)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var path = Path.Combine("Uploads/Doctors", fileName);

            Directory.CreateDirectory("Uploads/Doctors");

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            // save path to DoctorDocument table (service layer)
            return Ok("Document uploaded");
        }


        //***********************
        //Doctor Create staff
        //************************

        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/staff")]
        public async Task<IActionResult> CreateStaff(
    StaffCreateDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.CreateStaff(userId, dto);
            return Ok("Staff created & OTP sent");
        }


        //***********************
        //staff view queue for doctors
        //************************



        [Authorize(Roles = "Staff")]
        [HttpGet("staff/queue")]
        public async Task<IActionResult> GetStaffQueue(
    DateOnly date)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            return Ok(await _service.GetStaffQueue(userId, date));
        }











        // =========================
        // ADD DOCTOR AVAILABILITY
        // Doctor / Admin only
        // =========================

        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost("doctor/{doctorId}/slots")]
        public async Task<IActionResult> AddDoctorSlot(
            int doctorId,
            SlotCreateDto dto)
        {
            await _service.AddDoctorSlot(doctorId, dto);
            return Ok("Slot added");
        }



        // =========================
        // CHECK-IN & QUEUE
        // =========================

        [HttpPost("appointment/checkin/{token}")]
        public async Task<IActionResult> CheckIn(string token)
        {
            var queueNo = await _service.CheckIn(token);
            return Ok(new { queueNo });
        }

        // =========================
        // DOCTOR DASHBOARD (FINAL)
        // =========================

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/appointments")]
        public async Task<IActionResult> GetDoctorAppointments(
            [FromQuery] string type = "upcoming")
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            return Ok(await _service.GetDoctorAppointments(userId, type));
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/queue")]
        public async Task<IActionResult> GetDoctorQueue(DateOnly date)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            return Ok(await _service.GetDoctorQueue(userId, date));
        }

        // =========================
        // DOCTOR CONSULT
        // =========================

        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/consult")]
        public async Task<IActionResult> Consult([FromBody] DoctorConsultDto dto)
        {
            await _service.Consult(dto);
            return Ok("Consultation saved");
        }


        // =========================
        // ADMIN DASHBOARD
        // =========================

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/appointments")]
        public async Task<IActionResult> GetAdminAppointments(
            DateOnly date,
            [FromQuery] string? status)
        {
            int hospitalId = int.Parse(
                User.FindFirst("HospitalId")!.Value
            );

            var list = await _service.GetAdminAppointments(
                hospitalId, date, status);

            return Ok(list);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/queue")]
        public async Task<IActionResult> GetAdminQueue(
            DateOnly date)
        {
            int hospitalId = int.Parse(
                User.FindFirst("HospitalId")!.Value
            );

            return Ok(await _service.GetAdminQueue(hospitalId, date));
        }


        // =========================
        // PAYMENT DETAILS(ADMIN)
        // =========================


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/payment/{appointmentId}")]
        public async Task<IActionResult> GetPayment(int appointmentId)
        {
            var details = await _service.GetPaymentDetails(appointmentId);
            return Ok(details);
        }

        // =========================
        // CONFIRM PAYMENT (ADMIN)
        // =========================


        [Authorize(Roles = "Admin")]
        [HttpPost("admin/payment/confirm")]
        public async Task<IActionResult> ConfirmPayment(ConfirmPaymentDto dto)
        {
            await _service.ConfirmPayment(dto);
            return Ok("Payment completed");
        }




        // =========================
        // ADMIN VIEWS
        // =========================




        [Authorize(Roles = "Admin")]
        [HttpGet("admin/doctors")]
        public IActionResult GetDoctorsForAdmin()
        {
            int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);
            return Ok(_service.GetDoctorsForAdmin(hospitalId));
        }

        //Patient History For Doctor

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/patient/{patientId}/history")]
        public async Task<IActionResult> GetPatientHistoryForDoctor(int patientId)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var history = await _service
                .GetPatientHistoryForDoctor(doctorUserId, patientId);

            return Ok(history);
        }


        //Patient Own History


        [Authorize(Roles = "Patient")]
        [HttpGet("patient/history")]
        public async Task<IActionResult> GetMyHistory()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var history = await _service.GetPatientHistory(userId);

            return Ok(history);
        }

        //Update Patient Details

        [Authorize(Roles = "Patient")]
        [HttpPost("patient/personal-details")]
        public async Task<IActionResult> UpdatePatientPersonalDetails(
    PatientPersonalDetailsDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.UpdatePatientPersonalDetails(userId, dto);

            return Ok("Patient personal details saved successfully");
        }




    }
}
