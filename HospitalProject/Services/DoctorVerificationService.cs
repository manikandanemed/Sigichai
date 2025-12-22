//using HospitalProject.Models;
//using HospitalProject.Repositories;

//namespace HospitalProject.Services
//{
//    public class DoctorVerificationService
//    {
//        private readonly IRepository<Doctor> _doctor;
//        private readonly IRepository<DoctorVerification> _verification;

//        public DoctorVerificationService(
//            IRepository<Doctor> doctor,
//            IRepository<DoctorVerification> verification)
//        {
//            _doctor = doctor;
//            _verification = verification;
//        }

//        public async Task VerifyDoctorAsync(DoctorVerificationDto dto)
//        {
//            // 1️⃣ Doctor exists?
//            var doc = await _doctor.GetAsync(d => d.Id == dto.DoctorId);
//            if (doc == null)
//                throw new Exception("Doctor not found");

//            // 2️⃣ 🔴 TEMP: Assume IDfy success
//            bool idfySuccess = true;

//            // 3️⃣ Update Doctor table
//            doc.IsVerified = idfySuccess;
//            doc.VerificationStatus = idfySuccess ? "VERIFIED" : "FAILED";
//            await _doctor.SaveAsync();

//            // 4️⃣ Save verification history
//            await _verification.AddAsync(new DoctorVerification
//            {
//                DoctorId = doc.Id,
//                RegistrationNumber = dto.RegistrationNumber,
//                YearOfRegistration = dto.YearOfRegistration,
//                CouncilName = dto.CouncilName,
//                VerificationStatus = doc.VerificationStatus,
//                RawResponse = "IDfy verified (sample)"
//            });

//            await _verification.SaveAsync();
//        }
//    }
//}



using HospitalProject.Models;
using HospitalProject.Repositories;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HospitalProject.Services
{
    public class DoctorVerificationService
    {
        private readonly IRepository<Doctor> _doctor;
        private readonly IRepository<DoctorVerification> _verification;
        private readonly IConfiguration _config;

        public DoctorVerificationService(
            IRepository<Doctor> doctor,
            IRepository<DoctorVerification> verification,
            IConfiguration config)
        {
            _doctor = doctor;
            _verification = verification;
            _config = config;
        }

        public async Task VerifyDoctorAsync(DoctorVerificationDto dto)
        {
            // 1️⃣ Doctor exists check
            var doc = await _doctor.GetAsync(d => d.Id == dto.DoctorId);
            if (doc == null)
                throw new Exception("Doctor not found");

            // 2️⃣ IDfy API URL
            var url = $"{_config["IDfy:BaseUrl"]}/v3/tasks/async/verify_with_source/doctor";

            // 3️⃣ Request payload (IDfy format)
            var payload = new
            {
                task_id = Guid.NewGuid().ToString(),
                group_id = "hospital_app",
                data = new
                {
                    registration_number = dto.RegistrationNumber,
                    year_of_registration = dto.YearOfRegistration.ToString(),
                    council_name = dto.CouncilName
                }
            };

            using var client = new HttpClient();

            // 4️⃣ REQUIRED HEADERS
            client.DefaultRequestHeaders.Add("api-key", _config["IDfy:ApiKey"]);
            client.DefaultRequestHeaders.Add("account-id", _config["IDfy:AccountId"]);

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            // 5️⃣ Call IDfy
            var response = await client.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            // 6️⃣ VERY IMPORTANT: Parse REAL result
            // IDfy returns status ACTIVE for valid doctor
            bool isVerified =
                json.Contains("\"status\":\"ACTIVE\"") ||
                json.Contains("\"status\": \"ACTIVE\"");

            // 7️⃣ Update Doctor table
            doc.IsVerified = isVerified;
            doc.VerificationStatus = isVerified ? "VERIFIED" : "FAILED";
            await _doctor.SaveAsync();

            // 8️⃣ Save verification audit
            await _verification.AddAsync(new DoctorVerification
            {
                DoctorId = doc.Id,
                RegistrationNumber = dto.RegistrationNumber,
                YearOfRegistration = dto.YearOfRegistration,
                CouncilName = dto.CouncilName,
                VerificationStatus = doc.VerificationStatus,
                RawResponse = json,
                VerifiedOn = DateTime.UtcNow
            });

            await _verification.SaveAsync();

            // 9️⃣ If failed, throw error (IMPORTANT)
            if (!isVerified)
                throw new Exception("Doctor verification failed. Invalid registration details.");
        }
    }
}






