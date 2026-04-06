namespace HospitalProject.Services
{
    public interface IMsg91Service
    {
        Task SendOtpAsync(string mobile, string otp);
        Task SendAppointmentConfirmationAsync(
            string mobile,
            string token,
            string tentativeTime,
            string date,
            string doctorName,
            string hospitalName);
    }

}
