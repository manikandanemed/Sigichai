using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace HospitalProject.Services
{
    // =========================
    // INTERFACE
    // =========================
    public interface ITwilioService
    {
        Task SendOtpAsync(string mobileNumber, string otp);
    }

    // =========================
    // IMPLEMENTATION
    // =========================
    public class TwilioService : ITwilioService
    {
        private readonly IConfiguration _config;

        public TwilioService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpAsync(string mobileNumber, string otp)
        {
            var accountSid = _config["Twilio:AccountSid"];
            var authToken = _config["Twilio:AuthToken"];
            var messagingServiceSid = _config["Twilio:MessagingServiceSid"];

            TwilioClient.Init(accountSid, authToken);

            await MessageResource.CreateAsync(
                body: $"Your OTP is {otp}. Valid for 5 minutes.",
                messagingServiceSid: messagingServiceSid,
                to: new Twilio.Types.PhoneNumber(mobileNumber)
            );
        }
    }
}
