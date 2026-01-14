using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalProject.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous] // 🔥 ENTIRE CONTROLLER IS PUBLIC
    public class AuthController : ControllerBase
    {
        private readonly HospitalService _service;

        public AuthController(HospitalService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            await _service.Login(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "OTP sent successfully"
            });
        }


        //[HttpPost("verify-otp")]
        //public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        //{
        //    var result = await _service.VerifyOtp(dto);

        //    return Ok(new ApiResponse
        //    {
        //        Success = true,
        //        Message = "OTP verified",
        //        Data = new
        //        {
        //            token = result.Token,
        //            role = result.Role,
        //            userId = result.UserId,
        //            userName = result.Name
        //        }
        //    });
        //}


        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var result = await _service.VerifyOtp(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "OTP verified",
                Data = new
                {
                    token = result.Token,
                    role = result.Role,
                    name = result.Name,
                    userId = result.UserId,

                    adminId = result.AdminId,
                    doctorId = result.DoctorId,
                    patientId = result.PatientId
                }
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            await _service.ForgotPassword(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "OTP sent to registered mobile number"
            });
        }



        // RESET PASSWORD
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await _service.ResetPassword(dto);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Password reset successfully"
            });
        }
    }
}
