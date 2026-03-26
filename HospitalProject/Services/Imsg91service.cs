namespace HospitalProject.Services
{
    public interface IMsg91Service
    {
        Task SendOtpAsync(string mobile, string otp);
    }

}
