using Microsoft.SemanticKernel;
using System.ComponentModel;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

internal class Program
{
    private static void Main(string[] args)
    {
    }
}

[Description("This is SMS sending plugin")]
public class SMSPlugin
{
    public SMSPlugin()
    {        
        var username = Environment.GetEnvironmentVariable("TWILIO_USERNAME");
        var password = Environment.GetEnvironmentVariable("TWILIO_PASSWORD"); ;
        TwilioClient.Init(username, password);
    }

    [KernelFunction]
    [Description("Send SMS message with given text as content")]
    public async Task<string> SendSMSMessage([Description("text of SMS")] string text, [Description("phone number in format +CCXXXYYYZZZ")] string phone)
    {
        phone = $"{phone}";

        var twilioPhoneFrom = Environment.GetEnvironmentVariable("TWILIO_PHONE_FROM");

        var message = MessageResource.Create(
            new PhoneNumber(phone),
            from: new PhoneNumber(twilioPhoneFrom),
            body: text
        );

        return "message has been sent";
    }
}

