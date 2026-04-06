using System.Net.Http;
using System.Text;
using System.Text.Json;
using HospitalProject.Models;
using HospitalProject.Services;
using Microsoft.Extensions.Options;

public class Msg91Service : IMsg91Service
{
    private readonly HttpClient _httpClient;
    private readonly Msg91Settings _settings;

    public Msg91Service(HttpClient httpClient, IOptions<Msg91Settings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    private string FormatMobile(string mobile)
    {
        if (string.IsNullOrEmpty(mobile))
            return mobile;

        var cleaned = mobile.Replace("+", "").Replace(" ", "");

        // 10 digit India number → add 91 prefix
        if (cleaned.Length == 10)
            cleaned = "91" + cleaned;

        return cleaned;
    }

    public async Task SendOtpAsync(string mobile, string otp)
    {
        var formattedMobile = FormatMobile(mobile);

        var payload = new
        {
            integrated_number = _settings.SenderNumber,
            content_type = "template",
            payload = new
            {
                messaging_product = "whatsapp",
                to = formattedMobile,
                type = "template",
                template = new
                {
                    name = _settings.TemplateName,
                    language = new { code = "en" },
                    components = new object[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new object[]
                            {
                                new { type = "text", text = otp }
                            }
                        },
                        // 🔥 Authentication template ku button component REQUIRED
                        new
                        {
                            type = "button",
                            sub_type = "url",
                            index = "0",
                            parameters = new object[]
                            {
                                new { type = "text", text = otp }
                            }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        Console.WriteLine("MSG91 Request: " + json);

        var request = new HttpRequestMessage(HttpMethod.Post, _settings.WhatsAppUrl);
        request.Headers.Add("authkey", _settings.AuthKey);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine("MSG91 Response: " + result);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"MSG91 Error: {result}");
        }
    }


        public async Task SendAppointmentConfirmationAsync(
    string mobile,
    string token,
    string tentativeTime,
    string date,
    string doctorName,
    string hospitalName)
    {
        var formattedMobile = FormatMobile(mobile);

        var payload = new
        {
            integrated_number = _settings.SenderNumber,
            content_type = "template",
            payload = new
            {
                messaging_product = "whatsapp",
                to = formattedMobile,
                type = "template",
                template = new
                {
                    name = _settings.AppointmentTemplateName,
                    language = new { code = "en" },
                    components = new object[]
                    {
                    new
                    {
                        type = "body",
                        parameters = new object[]
                        {
                            new { type = "text", text = token },          // {{1}}
                            new { type = "text", text = tentativeTime },  // {{2}}
                            new { type = "text", text = date },           // {{3}}
                            new { type = "text", text = doctorName },     // {{4}}
                            new { type = "text", text = hospitalName }    // {{5}}
                        }
                    }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        Console.WriteLine("MSG91 Appointment Confirmation Request: " + json);

        var request = new HttpRequestMessage(HttpMethod.Post, _settings.WhatsAppUrl);
        request.Headers.Add("authkey", _settings.AuthKey);
        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine("MSG91 Response: " + result);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"MSG91 Error: {result}");
        }
}
