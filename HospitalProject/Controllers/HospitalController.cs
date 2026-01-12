using System.Collections.Generic;
using System.Security.Claims;
using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Twilio.Jwt.AccessToken;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Patient registered successfully"
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateHospital(
        HospitalCreateDto dto)
        {
            var hospitalId = await _service.CreateHospital(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Hospital created successfully",
                Data = new
                {
                    hospitalId = hospitalId
                }
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
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Hospital doctor registered. Pending verification"
            });
        }


        [HttpPost("doctor/register/independent")]
        public async Task<IActionResult> RegisterIndependentDoctor(
    IndependentDoctorRegDto dto)
        {
            await _service.RegisterIndependentDoctor(dto);
       
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Independent doctor registered. Pending verification"
            });
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

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Admin created for your clinic & OTP sent"
            });
        }



        //SUPER ADMIN

        [HttpPost("hospital")]
        public async Task<IActionResult> SetupHospital(
        HospitalSetupDto dto)
        {
            await _service.SetupHospitalWithAdmin(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Hospital & First Admin created"
            });
           
        }






        [Authorize(Roles = "Admin")]
        [HttpPost("hospital/admin/register")]
        public async Task<IActionResult> RegisterAdmin(AdminRegDto dto)
        {
            int hospitalId = int.Parse(
                User.FindFirst("HospitalId")!.Value
            );

            await _service.RegisterAdmin(hospitalId, dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Admin registered"
            });
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

            return Ok(new ApiResponse
            {
                Success = true,
                Data = doctors
            });


        }

        [HttpGet("doctor/{doctorId}/slots")]
        public async Task<IActionResult> GetSlots(
     int doctorId,
     DateOnly date)
        {
            var slots = await _service.GetAvailableSlots(doctorId, date);
            return Ok(new ApiResponse
            {
                Success = true,
                Data = slots
            });
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
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Appointment booked successfully",
                Data = new
                {
                    tempToken = token
                }
            });

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

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Profile fetched successfully",
                Data = profile
            });

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

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Profile updated successfully"
            });
            
        }

        // =========================
        //  Update Vitals by Admin
        // =========================

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/appointment/update-vitals")]
        public async Task<IActionResult> UpdateVitalsByAdmin(
    UpdateVitalsDto dto)
        {
            int adminUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.UpdateVitalsByAdmin(adminUserId, dto);
  
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Vitals updated successfully"
            });
        }

        // =========================
        //  Doctor View Patient Vitals
        // =========================

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/patient/workspace/{patientUserId}")]
        public async Task<IActionResult> GetPatientWorkspace(
    int patientUserId)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var data = await _service
                .GetPatientWorkspaceForDoctor(doctorUserId, patientUserId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "patient details fetched successfully",
                Data = data
            });
        }


        // =========================
        // Public api for get user details
        // =========================



        [HttpGet("public/user/{userId}")]
        public async Task<IActionResult> GetPublicUserDetails(int userId)
        {
            var data = await _service.GetPublicUserDetails(userId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Users details fetched successfully",
                Data = data
            });
        }




        // =========================
        // PUBLIC APIs (NO AUTH)
        // =========================

        //[HttpGet("public/hospitals")]
        //public async Task<IActionResult> GetAllHospitals()
        //{
        //    return Ok(await _service.GetAllHospitals());
        //}

        //[HttpGet("public/hospital/{hospitalId}/doctors")]
        //public async Task<IActionResult> GetDoctorsByHospital(int hospitalId)
        //{
        //    return Ok(await _service.GetDoctorsByHospital(hospitalId));
        //}

        //[HttpGet("public/specialities")]
        //public async Task<IActionResult> GetSpecialities()
        //{
        //    return Ok(await _service.GetSpecialities());
        //}



        [HttpGet("public/hospitals")]
        public async Task<IActionResult> GetAllHospitals()
        {
            var hospitals = await _service.GetAllHospitals();

            return Ok(new ApiResponse
            {
                Success = true,
                Data = hospitals
            });
        }

        [HttpGet("public/hospital/{hospitalId}/doctors")]
        public async Task<IActionResult> GetDoctorsByHospital(int hospitalId)
        {
            var doctors = await _service.GetDoctorsByHospital(hospitalId);

            return Ok(new ApiResponse
            {
                Success = true,
                Data = doctors
            });
        }

        [HttpGet("public/specialities")]
        public async Task<IActionResult> GetSpecialities()
        {
            var specialities = await _service.GetSpecialities();

            return Ok(new ApiResponse
            {
                Success = true,
                Data = specialities
            });
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
          
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Appointments details get successfully",
                Data = list
            });
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

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Appointment booked successfully",
                Data = new
                {
                    tempToken = token
                }
            });
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

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Family member added"
            });
        }




        // =========================
        // ONLINE BOOKINGS LIST (BOOKED)
        // =========================



        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("appointments/booked")]
        public async Task<IActionResult> GetBookedAppointments(DateOnly date)
        {
            var list = await _service.GetOnlineBookingsByDate(date);
            
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Get Booked Appointments details  successfully",
                Data = list
            });

        }



        // =========================
        //CHECK-IN LIST + QUEUE
        // ===========================


        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("appointments/checkedin")]
        public async Task<IActionResult> GetCheckedInAppointments(DateOnly date)
        {
            var list = await _service.GetCheckedInAppointmentsByDate(date);
         
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Checked in Details",
                Data = list
            });
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
            

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Doctor profile created"
            });
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
           
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Document uploaded"
            });
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

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Staff created & OTP sent"
            });
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

            var staffqueue = await _service.GetStaffQueue(userId, date);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Staff Queue list",
                Data = staffqueue
            });
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
           
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Slot added"
            });

        }



        // =========================
        // CHECK-IN & QUEUE
        // =========================

        [HttpPost("appointment/checkin/{token}")]
        public async Task<IActionResult> CheckIn(string token)
        {
            var queueNo = await _service.CheckIn(token);
        
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Check in Successfully",
                Data = queueNo
            });
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

            var appointments = await _service.GetDoctorAppointments(userId, type);

            return Ok(new ApiResponse
            {
                Success = true,
                //Message = "Apontments Details",
                Data = appointments
            });


        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/queue")]
        public async Task<IActionResult> GetDoctorQueue(DateOnly date)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var Queue = await _service.GetDoctorQueue(userId, date);

            return Ok(new ApiResponse
            {
                Success = true,
                //Message = "Apontments Details",
                Data = Queue
            });

           
        }

        // =========================
        // DOCTOR CONSULT
        // =========================

        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/consult")]
        public async Task<IActionResult> Consult([FromBody] DoctorConsultDto dto)
        {
            await _service.Consult(dto);
            
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Consultation saved",
                
            });
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


            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Get Apontments Details Successfully",
                Data = list
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/queue")]
        public async Task<IActionResult> GetAdminQueue(
            DateOnly date)
        {
            int hospitalId = int.Parse(
                User.FindFirst("HospitalId")!.Value
            );

            var adminqueue = await _service.GetAdminQueue(hospitalId, date);


            return Ok(new ApiResponse
            {
                Success = true,
                //Message = "Apontments Details",
                Data = adminqueue
            });
        }


        // =========================
        // PAYMENT DETAILS(ADMIN)
        // =========================


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/payment/{appointmentId}")]
        public async Task<IActionResult> GetPayment(int appointmentId)
        {
            var details = await _service.GetPaymentDetails(appointmentId);
            
            return Ok(new ApiResponse
            {
                Success = true,
                //Message = "Apontments Details",
                Data = details
            });
        }

        // =========================
        // CONFIRM PAYMENT (ADMIN)
        // =========================


        [Authorize(Roles = "Admin")]
        [HttpPost("admin/payment/confirm")]
        public async Task<IActionResult> ConfirmPayment(ConfirmPaymentDto dto)
        {
            await _service.ConfirmPayment(dto);
         
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Payment completed"
                
            });
        }




        // =========================
        // ADMIN VIEWS
        // =========================




        [Authorize(Roles = "Admin")]
        [HttpGet("admin/doctors")]
        public IActionResult GetDoctorsForAdmin()
        {
            int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);

            var doctors =  _service.GetDoctorsForAdmin(hospitalId);

            return Ok(new ApiResponse
            {
                Success = true,
                Data = doctors
            });
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


            return Ok(new ApiResponse
            {
                Success = true,
                Data = history
            });
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


            return Ok(new ApiResponse
            {
                Success = true,
                Data = history
            });
        }

        //Update Patient Details

        //[Authorize(Roles = "Patient")]
        //[HttpPost("patient/personal-details")]
        //public async Task<IActionResult> UpdatePatientPersonalDetails(
        // PatientPersonalDetailsDto dto)
        //{
        //    int userId = int.Parse(
        //        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        //    );

        //    await _service.UpdatePatientPersonalDetails(userId, dto);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Patient details updated successfully"
        //    });
        //}


        [Authorize(Roles = "Patient")]
        [HttpPut("patient/personal-details")]
        public async Task<IActionResult> UpdatePatientPersonalDetails(
    UpdatePatientPersonalDetailsDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.UpdatePatientPersonalDetails(userId, dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Patient personal details updated successfully"
            });
        }




        // =========================
        // PATIENT PERSONAL DETAILS DOCTOR VIEW
        // =========================


        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/patient/{patientId}/details")]
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var details = await _service
                .GetPatientBasicDetailsForDoctor(doctorUserId, patientId);

       

            return Ok(new ApiResponse
            {
                Success = true,
                Data = details
            });
        }




        // =========================
        // Doctor Arrived send message to patient
        // =========================

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/doctor/{doctorId}/arrived")]
        public async Task<IActionResult> MarkDoctorArrived(int doctorId)
        {
            int adminUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.MarkDoctorArrived(adminUserId, doctorId);

            
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Doctor marked as arrived and patients notified"
            });
        }



        [Authorize(Roles = "Patient")]
        [HttpGet("patient/personal-details")]
        public async Task<IActionResult> GetPatientPersonalDetails()
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var data = await _service.GetPatientPersonalDetails(userId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Patient personal details fetched successfully",
                Data = data
            });
        }



        // patient can view queue line details

        [Authorize(Roles = "Patient")]
        [HttpGet("patient/queue-status/{tempToken}")]
        public async Task<IActionResult> GetPatientQueueStatus(string tempToken)
        {
            var status = await _service
                .GetPatientQueueStatusByTempToken(tempToken);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Queue status fetched successfully",
                Data = status
            });
        }







    }
}
