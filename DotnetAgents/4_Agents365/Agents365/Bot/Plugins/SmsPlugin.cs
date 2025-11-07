namespace Agents365.Bot.Plugins
{
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Numerics;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
[Description("This is SMS sending plugin")]
public class SMSPlugin
{
    public SMSPlugin()
    {
        var username = "SK1f3c99abcb59f270236f12157c6dd286";
        var password = "GfYVBp4s4ascW5WBMi738iuWrwLoA1m";
        TwilioClient.Init(username, password);
    }

    [KernelFunction]
    [Description("Send SMS message with given text as content")]
    public async Task<string> SendSMSMessage([Description("text of SMS")] string text, [Description("phone number in format +CCXXXYYYZZZ")] string phone)
    {
        phone = $"{phone}";

        var message = MessageResource.Create(
            new PhoneNumber(phone),
            from: new PhoneNumber("+48732059666"),
            body: text
        );

        return "message has been sent";
    }
}

}
