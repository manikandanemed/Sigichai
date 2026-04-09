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
        private readonly InternalPharmacyService _pharmacyService;

        public HospitalController(HospitalService service, InternalPharmacyService pharmacyService)
        {
            _service = service;
            _pharmacyService = pharmacyService;
        }

        // =========================
        // REGISTRATION APIs
        // =========================

        [HttpPost("register/patient")]
        public async Task<IActionResult> RegisterPatient(PatientRegDto dto)
        {
            try
            {
                await _service.RegisterPatient(dto);
                return Ok(new ApiResponse { Success = true, Message = "Patient registered" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
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


        // =====================================================================
        // 🔐 PRODUCT ADMIN REGISTER (One time only)
        // POST /api/hospital/register/product-admin
        // =====================================================================
        [HttpPost("register/product-admin")]
        public async Task<IActionResult> RegisterProductAdmin(
            [FromBody] ProductAdminRegisterDto dto)
        {
            try
            {
                await _service.RegisterProductAdmin(dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "ProductAdmin registered successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
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

        //[Authorize(Roles = "Admin")]
        [HttpPost("speciality/add")]
        public async Task<IActionResult> AddSpeciality(SpecialityCreateDto dto)
        {
            await _service.AddSpeciality(dto);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Speciality added successfully"
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
        public async Task<IActionResult> GetSlots(int doctorId)
        {
            try
            {
                var slots = await _service.GetAvailableSlots(doctorId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = slots.Any()
                        ? "Available slots fetched successfully"
                        : "No slots found",
                    Data = slots
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
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

        //[Authorize(Roles = "Patient")]
        //[HttpGet("patient/profile")]
        //public async Task<IActionResult> GetPatientProfile()
        //{
        //    int userId = int.Parse(
        //        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        //    );

        //    var profile = await _service.GetPatientProfile(userId);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Profile fetched successfully",
        //        Data = profile
        //    });

        //}

        //[Authorize(Roles = "Patient")]
        //[HttpPut("patient/profile")]
        //public async Task<IActionResult> UpdatePatientProfile(
        //    UpdatePatientProfileDto dto)
        //{
        //    int userId = int.Parse(
        //        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        //    );

        //    await _service.UpdatePatientProfile(userId, dto);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Profile updated successfully"
        //    });
            
        //}

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
        // Doctor View Patient Vitals (Appointment based)
        // =========================


        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/appointment/{appointmentId}/vitals")]
        public async Task<IActionResult> GetPatientVitalsByAppointment(
            int appointmentId)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var data = await _service
                .GetPatientVitalsForDoctorByAppointment(
                    doctorUserId, appointmentId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Patient vitals fetched successfully",
                Data = data
            });
        }



        // =========================
        // UPDATE VITALS BY DOCTOR
        // =========================

        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/appointment/update-vitals")]
        public async Task<IActionResult> UpdateVitalsByDoctor(
        UpdateVitalsDto dto)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.UpdateVitalsByDoctor(doctorUserId, dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Vitals updated by doctor successfully"
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



        // =====================================================================
        // 🏥 PRODUCT ADMIN — Bulk Hospital Create
        // POST /api/hospital/bulk-create
        // =====================================================================
        //[Authorize(Roles = "ProductAdmin")]
        [HttpPost("bulk-create")]
        public async Task<IActionResult> BulkCreateHospitals(
            [FromBody] BulkHospitalCreateDto dto)
        {
            try
            {
                var result = await _service.BulkCreateHospitals(dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // =====================================================================
        // 🌐 PUBLIC — Get All Hospitals
        // GET /api/hospital/all-hospitals
        // =====================================================================
        [HttpGet("all-hospitals")]
        public async Task<IActionResult> GetAllHospitals()
        {
            try
            {
                var result = await _service.GetAllHospitals();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
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



        //[Authorize(Roles = "Doctor,Admin")]
        //[HttpGet("appointments/booked")]
        //public async Task<IActionResult> GetBookedAppointments(DateOnly date)
        //{
        //    var list = await _service.GetOnlineBookingsByDate(date);
            
        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Get Booked Appointments details  successfully",
        //        Data = list
        //    });

        //}



        // =========================
        //CHECK-IN LIST + QUEUE
        // ===========================


        //[Authorize(Roles = "Doctor,Admin")]
        //[HttpGet("appointments/checkedin")]
        //public async Task<IActionResult> GetCheckedInAppointments(DateOnly date)
        //{
        //    var list = await _service.GetCheckedInAppointmentsByDate(date);
         
        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Checked in Details",
        //        Data = list
        //    });
        //}

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



        //********************************
        //Doctor profile View get Method
        //********************************

        //[Authorize(Roles = "Doctor")]
        //[HttpGet("doctor/profile")]
        //public async Task<IActionResult> GetDoctorProfile()
        //{
        //    int userId = int.Parse(
        //        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        //    );

        //    var data = await _service.GetDoctorProfile(userId);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Doctor profile fetched successfully",
        //        Data = data
        //    });
        //}



        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/profile")]
        public async Task<IActionResult> GetDoctorProfile()
        {
            try
            {
                var userId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var result = await _service.GetDoctorProfile(userId);

                // 👇 Profile இல்லன்னா empty response
                if (result == null)
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Doctor profile not created yet",
                        Data = null
                    });

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Doctor profile fetched successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }



        //********************************
        //Doctor profile View Put Method
        //********************************

        [Authorize(Roles = "Doctor")]
        [HttpPatch("doctor/profile")]
        public async Task<IActionResult> UpdateDoctorProfile(
     [FromBody] DoctorProfileUpdateDto dto)
        {
            try
            {
                var userId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                await _service.UpdateDoctorProfile(userId, dto);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Doctor profile updated"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
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
        public async Task<IActionResult> CreateStaff(StaffCreateDto dto)
        {
            try
            {
                int userId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!
                );

                await _service.CreateStaff(userId, dto);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"{dto.StaffRole} created successfully & OTP sent"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        //***********************
        //staff view queue for doctors
        //************************



        [Authorize(Roles = "Admin,Staff,Nurse")]
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

        //[Authorize(Roles = "Doctor,Admin")]
        //[HttpPost("doctor/{doctorId}/slots")]
        //public async Task<IActionResult> AddDoctorSlot(
        //    int doctorId,
        //    SlotCreateDto dto)
        //{
        //    await _service.AddDoctorSlot(doctorId, dto);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Slot added"
        //    });

        //}


        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost("doctor/{doctorId}/slots")]
        public async Task<IActionResult> AddDoctorSlot(
    int doctorId,
    [FromBody] BulkSlotCreateDto dto)
        {
            try
            {
                await _service.AddDoctorSlot(doctorId, dto);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Slots added successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        // =========================
        // CHECK-IN & QUEUE
        // =========================
        [Authorize(Roles = "Patient,Admin")]
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

        //[Authorize(Roles = "Doctor")]
        //[HttpGet("doctor/appointments")]
        //public async Task<IActionResult> GetDoctorAppointments(
        //    [FromQuery] string type = "upcoming")
        //{
        //    int userId = int.Parse(
        //        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        //    );

        //    var appointments = await _service.GetDoctorAppointments(userId, type);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        //Message = "Apontments Details",
        //        Data = appointments
        //    });


        //}

        //[Authorize(Roles = "Doctor")]
        //[HttpGet("doctor/queue")]
        //public async Task<IActionResult> GetDoctorQueue(DateOnly date)
        //{
        //    int userId = int.Parse(
        //        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        //    );

        //    var Queue = await _service.GetDoctorQueue(userId, date);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        //Message = "Apontments Details",
        //        Data = Queue
        //    });

           
        //}

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
                Message = "Consultation and Prescription saved",
            });
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/pharmacy/search")]
        public async Task<IActionResult> SearchPharmacyMedicines([FromQuery] string? query)
        {
            var result = await _pharmacyService.GetAvailableMedicines(query);
            return Ok(new ApiResponse
            {
                Success = true,
                Data = result
            });
        }


        // =========================
        // ADMIN DASHBOARD
        // =========================




        //************************************************
        // Admin view appointments with date status slot 
        //************************************************


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/appointments")]
        public async Task<IActionResult> GetAdminAppointments(
    [FromQuery] DateOnly? date,
    [FromQuery] string? status,
    [FromQuery] string? timeSlot)
        {
            var list = await _service.GetAdminAppointments(
                date, status, timeSlot);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Appointments fetched successfully",
                Data = list
            });
        }




        /*// Queue seperate not used
            [Authorize(Roles = "Admin")]
            [HttpGet("admin/queue")]
            public async Task<IActionResult> GetAdminQueue(DateOnly date)
            {
                var list = await _service.GetAdminQueue(date);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Checked-in queue fetched successfully",
                    Data = list
                });
            }



            [Authorize(Roles = "Doctor")]
            [HttpGet("doctor/queuee")]
            public async Task<IActionResult> GetDoctorQueuee(DateOnly date)
            {
                int userId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!
                );

                var list = await _service.GetDoctorQueuee(userId, date);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Doctor queue fetched successfully",
                    Data = list
                });
            }

            // Queue seperate not used*/




        //************************************************
        // Doctor view appointments with date status slot 
        //************************************************

        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/appointmentss")]
        public async Task<IActionResult> GetDoctorAppointments(
        [FromQuery] DateOnly? date,
        [FromQuery] string? status,
        [FromQuery] string? timeSlot)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var list = await _service.GetDoctorAppointmentss(
                userId, date, status, timeSlot);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Doctor appointments fetched successfully",
                Data = list
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




        //[Authorize(Roles = "Admin")]
        //[HttpGet("admin/doctors")]
        //public IActionResult GetDoctorsForAdmin()
        //{
        //    int hospitalId = int.Parse(User.FindFirst("HospitalId")!.Value);

        //    var doctors =  _service.GetDoctorsForAdmin(hospitalId);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Data = doctors
        //    });
        //}



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

        //[Authorize(Roles = "Admin")]
        //[HttpPost("admin/doctor/{doctorId}/arrived")]
        //public async Task<IActionResult> MarkDoctorArrived(int doctorId)
        //{
        //    int adminUserId = int.Parse(
        //        User.FindFirstValue(ClaimTypes.NameIdentifier)!
        //    );

        //    await _service.MarkDoctorArrived(adminUserId, doctorId);

            
        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Doctor marked as arrived and patients notified"
        //    });
        //}



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



        //***************************************
        // Admin view doctor details for Add slot
        //***************************************


        [Authorize(Roles = "Admin")]
        [HttpGet("admin/doctors")]
        public async Task<IActionResult> GetDoctorsForAdmin()
        {
            var list = await _service.GetDoctorsForAdmin();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Doctors fetched successfully",
                Data = list
            });
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("admin/appointments/mark-noshow")]
        public async Task<IActionResult> MarkNoShowByAdmin(
        MarkNoShowDto dto)
        {
            var count = await _service.MarkNoShowBySlot(
                dto.Date, dto.TimeSlot);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"{count} appointments marked as NoShow",
                Data = count
            });
        }


        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/appointments/mark-noshow")]
        public async Task<IActionResult> MarkNoShowByDoctor(
    MarkNoShowDto dto)
        {
            var count = await _service.MarkNoShowBySlot(
                dto.Date, dto.TimeSlot);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"{count} appointments marked as NoShow",
                Data = count
            });
        }



        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/block-admin/{adminUserId}")]
        public async Task<IActionResult> BlockAdmin(int adminUserId)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.BlockAdminByDoctor(doctorUserId, adminUserId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Admin blocked successfully"
            });
        }



        [Authorize(Roles = "Doctor")]
        [HttpPost("doctor/unblock-admin/{adminUserId}")]
        public async Task<IActionResult> UnblockAdmin(int adminUserId)
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            await _service.UnblockAdminByDoctor(doctorUserId, adminUserId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Admin unblocked successfully"
            });
        }



        // ==================================
        // Doctor → View Admin List
        // ==================================
        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/admins")]
        public async Task<IActionResult> GetAdminsForDoctor()
        {
            int doctorUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var list = await _service.GetAdminsForDoctor(doctorUserId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Admins fetched successfully",
                Data = list
            });
        }


        //END SESION GET BY SLOT ID

        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost("session/end-by-slot")]
        public async Task<IActionResult> EndSessionBySlot(
        EndSessionBySlotDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var role = User.FindFirstValue(ClaimTypes.Role)!;

            await _service.EndSessionBySlot(userId, role, dto.SlotId);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Session ended successfully"
            });
        }


        //[HttpPost("book/self")]
        //public async Task<IActionResult> BookSelf(PatientTimeBookingDto dto)
        //{
        //    int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        //    var request = new BookAppointmentDto
        //    {
        //        DoctorId = dto.DoctorId,
        //        Date = dto.Date,
        //        TimeSlot = dto.TimeSlot,
        //        ReasonForVisit = dto.ReasonForVisit,
        //        FamilyMemberId = null
        //    };

        //    var result = await _service.BookWithPayment(userId, request, false);

        //    return Ok(result);
        //}


        // =====================================================================
        // 👤 PATIENT — Nearby Hospitals
        // GET /api/hospital/nearby-hospitals?lat=xx&lon=xx
        // =====================================================================

        //[Authorize(Roles = "Patient")]
        //[HttpGet("nearby-hospitals")]
        //public async Task<IActionResult> GetNearbyHospitals(
        //[FromQuery] double lat,
        //[FromQuery] double lon,
        //[FromQuery] int? specialityId = null,
        //[FromQuery] double maxDistanceKm = 5,
        //[FromQuery] int page = 1,
        //[FromQuery] int pageSize = 10)
        //    {
        //    try
        //    {
        //        var result = await _service.GetNearbyHospitals(
        //        lat, lon, specialityId, maxDistanceKm, page, pageSize);

        //        return Ok(new ApiResponse
        //        {
        //            Success = true,
        //            Data = result
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new ApiResponse
        //        {
        //            Success = false,
        //            Message = ex.Message
        //        });
        //    }
        //}


        [Authorize(Roles = "Patient")]
        [HttpGet("nearby-hospitals")]
        public async Task<IActionResult> GetNearbyHospitals(
    [FromQuery] double lat,
    [FromQuery] double lon,
    [FromQuery] int? specialityId = null,
    [FromQuery] double maxDistanceKm = 5,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] DateOnly? date = null)  // ✅ add
        {
            try
            {
                var result = await _service.GetNearbyHospitals(
                    lat, lon, specialityId, maxDistanceKm, page, pageSize, date);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Hospitals fetched successfully",
                    Data = result  // ✅ TotalPages, TotalCount 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [Authorize(Roles = "Patient")]
        [HttpPost("book/self")]
        public async Task<IActionResult> BookSelf(PatientTimeBookingDto dto)
        {
            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var request = new BookAppointmentDto
            {
                HospitalId = dto.HospitalId,
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                TimeSlot = dto.TimeSlot,
                ReasonForVisit = dto.ReasonForVisit,
                FamilyMemberId = null
            };

            var result = await _service.BookWithPayment(userId, request, false);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Pay online and get Temptoken",
                Data = result   // 👈 direct service result
            });
        }


        [Authorize(Roles = "Patient")]
        [HttpPost("book/family")]
        public async Task<IActionResult> BookFamily(FamilyTimeBookingDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var request = new BookAppointmentDto
            {
                HospitalId = dto.HospitalId,
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                TimeSlot = dto.TimeSlot,
                ReasonForVisit = dto.ReasonForVisit,
                FamilyMemberId = dto.FamilyMemberId
            };

            var result = await _service.BookWithPayment(userId, request, true);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Pay online and get Temptoken",
                Data = result   // 👈 direct service result
            });
        }


        // ======================================
        // ❌ CANCEL APPOINTMENT
        // POST /api/hospital/appointment/{appointmentId}/cancel
        // ======================================
        [HttpPost("appointment/cancel")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CancelAppointment(CancelAppointmentDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Unauthorized"
                    });

                int userId = int.Parse(userIdClaim);

                var result = await _service.CancelAppointmentAsync(userId, dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                // ✅ Inner exception-உம் காட்டும்
                var fullError = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = fullError
                });
            }
        }

        //chatgpt
        //[Authorize(Roles = "Patient")]
        //[HttpPost("appointment/cancel")]
        //public async Task<IActionResult> CancelAppointment([FromBody] CancelAppointmentDto dto)
        //{
        //    try
        //    {
        //        if (dto == null)
        //        {
        //            return BadRequest(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "Request body is required"
        //            });
        //        }

        //        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        //        if (userIdClaim == null)
        //        {
        //            return Unauthorized(new ApiResponse
        //            {
        //                Success = false,
        //                Message = "User not authenticated properly"
        //            });
        //        }

        //        var userId = int.Parse(userIdClaim.Value);

        //        var result = await _service.CancelAppointmentAsync(userId, dto);

        //        return Ok(new ApiResponse
        //        {
        //            Success = true,
        //            Message = result.Message,
        //            Data = result
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new ApiResponse
        //        {
        //            Success = false,
        //            Message = ex.InnerException?.Message ?? ex.Message
        //        });
        //    }
        //}


        // =====================================================================
        // 👨‍⚕️ DOCTOR — NMC Auto Fill
        // GET /api/hospital/doctor/nmc-lookup/{registrationNumber}
        // =====================================================================
        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor/nmc-lookup/{registrationNumber}")]
        public async Task<IActionResult> NmcLookup(string registrationNumber)
        {
            try
            {
                var result = await _service.GetNmcRecord(registrationNumber);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }



        // ======================================
        // 📋 GET DOCTOR SLOTS (Grouped View)
        // GET /api/hospital/doctor/{doctorId}/slots
        // ======================================

        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("doctor/{doctorId}/slot-groups")]
        public async Task<IActionResult> GetDoctorSlots(int doctorId)
        {
            try
            {
                var data = await _service.GetDoctorSlots(doctorId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Doctor slots fetched successfully",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        // ======================================
        // 📋 GET DOCTOR SLOTS BY DATE
        // GET /api/hospital/doctor/{doctorId}/slots/by-date?date=2026-03-30
        // ======================================
        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet("doctor/{doctorId}/slots/by-date")]
        public async Task<IActionResult> GetDoctorSlotsByDate(
            int doctorId,
            [FromQuery] DateOnly date)
        {
            try
            {
                var data = await _service.GetDoctorSlotsByDate(doctorId, date);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Slots fetched successfully",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }



        // =====================================================================
        // 👨‍⚕️ DOCTOR — Update Slot
        // PUT /api/hospital/doctor/{doctorId}/slots/{slotId}
        // =====================================================================
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPut("doctor/{doctorId}/slots/{slotId}")]
        public async Task<IActionResult> UpdateSlot(
            int doctorId, int slotId, [FromBody] SlotUpdateDto dto)
        {
            try
            {
                await _service.UpdateSlot(doctorId, slotId, dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Slot updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // =====================================================================
        // 👨‍⚕️ DOCTOR — Delete Slot
        // DELETE /api/hospital/doctor/{doctorId}/slots/{slotId}
        // =====================================================================


        [Authorize(Roles = "Doctor,Admin")]
        [HttpDelete("doctor/{doctorId}/slots/{slotGroupId}")]
        public async Task<IActionResult> DeleteSlot(
        int doctorId,
        int slotGroupId,
       [FromQuery] int? availabilityId = null,
       [FromQuery] DateOnly? date = null)
        {
            try
            {
                await _service.DeleteSlot(doctorId, slotGroupId, date, availabilityId);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = date.HasValue
                        ? "Slot deleted successfully"
                        : "Slot group deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [Authorize(Roles = "Doctor,Admin")]
        [HttpDelete("doctor/{doctorId}/slots/partial")]
        public async Task<IActionResult> DeletePartialSlot(
    int doctorId,
    SlotPartialDeleteDto dto)
        {
            try
            {
                await _service.DeletePartialSlotAsync(doctorId, dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Slot updated successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [Authorize(Roles = "Patient")]
        [HttpGet("doctor-availability")]
        public async Task<IActionResult> GetDoctorAvailability(
     [FromQuery] int? hospitalId,      // 👈 optional
     [FromQuery] int? doctorId,
     [FromQuery] int? specialityId,
     [FromQuery] DateOnly? date)
        {
            try
            {
                var result = await _service.GetDoctorAvailability(
                    hospitalId, doctorId, specialityId, date);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        //**********************
        // Delete account soft
        //**********************

        [HttpDelete("account/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount(DeleteAccountDto dto)
        {
            try
            {
                var userId = int.Parse(
                    User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                await _service.DeleteAccountAsync(userId, dto);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Account deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        // API 1 — Hospital Select/Create
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost("doctor/{doctorId}/location/select")]
        public async Task<IActionResult> SelectHospital(
            int doctorId,
            [FromBody] HospitalSelectDto dto)
        {
            try
            {
                var hospitalId = await _service.SelectOrCreateHospitalAsync(doctorId, dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Hospital selected successfully",
                    Data = new { hospitalId }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // API 2 — Slot Create
        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost("doctor/{doctorId}/slots/new")]
        public async Task<IActionResult> AddDoctorSlotNew(
            int doctorId,
            [FromBody] SlotCreateNewDto dto)
        {
            try
            {
                await _service.AddDoctorSlotNew(doctorId, dto);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Slots added successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [Authorize(Roles = "Doctor")]
        [HttpDelete("doctor/{doctorId}/assigned-hospital/{hospitalId}")]
        public async Task<IActionResult> RemoveDoctorHospital(int doctorId, int hospitalId)
        {
            try
            {
                await _service.RemoveDoctorHospitalAsync(doctorId, hospitalId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Hospital removed successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

       

        //// =====================================================================
        //// SWITCH ROLE- UI dropdown availability roles
        //// =====================================================================
        //[Authorize]
        //[HttpGet("available-roles")]
        //public async Task<IActionResult> GetAvailableRoles()
        //{
        //    try
        //    {
        //        int userId = int.Parse(
        //            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        //        var result = await _service.GetAvailableRoles(userId);

        //        var currentRole = User.FindFirstValue(ClaimTypes.Role);

        //        return Ok(new ApiResponse
        //        {
        //            Success = true,
        //            Message = "Available roles fetched successfully",
        //            Data = new
        //            {
        //                currentRole = currentRole,
        //                availableRoles = ((dynamic)result).availableRoles
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new ApiResponse
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //            Data = null
        //        });
        //    }
        //}



        //// =====================================================================
        //// SWITCH ROLE
        //// =====================================================================

        //[Authorize]
        //[HttpPost("switch-role")]
        //public async Task<IActionResult> SwitchRole([FromBody] SwitchRoleDto dto)
        //{
        //    try
        //    {
        //        int userId = int.Parse(
        //            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        //        var token = await _service.SwitchRole(userId, dto.TargetRole);

        //        return Ok(new ApiResponse
        //        {
        //            Success = true,
        //            Message = $"Switched to {dto.TargetRole} successfully",
        //            Data = new
        //            {
        //                token = token,
        //                activeRole = dto.TargetRole
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new ApiResponse
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //            Data = null
        //        });
        //    }
        //}

        //// =====================================================================
        //// SWITCH ROLE-current role view
        //// =====================================================================

        //[Authorize]
        //[HttpGet("current-role")]
        //public IActionResult GetCurrentRole()
        //{
        //    var role = User.FindFirstValue(ClaimTypes.Role);
        //    var originalRole = User.FindFirst("OriginalRole")?.Value;
        //    var doctorId = User.FindFirst("DoctorId")?.Value;
        //    var staffId = User.FindFirst("StaffId")?.Value;

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "Current role fetched successfully",
        //        Data = new
        //        {
        //            activeRole = role,
        //            originalRole = originalRole,
        //            doctorId = doctorId,
        //            staffId = staffId
        //        }
        //    });
        //}

        







    }
}
