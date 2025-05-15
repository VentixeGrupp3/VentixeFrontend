using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using WebApp.Protos;

namespace WebApp.Services;

public interface IVerificationService
{

}
public class VerificationService(EmailConfirmation.EmailConfirmationClient client, IConfiguration configuration)
{
    private readonly EmailConfirmation.EmailConfirmationClient _client = client;
    private readonly IConfiguration _configuration = configuration;

    public async Task<EmailConfirmationResponse> SendEmailAsync(string email)
    {
        var request = new EmailConfirmationRequest() { Email = email };
        var serviceBusClient = new ServiceBusClient(_configuration.GetConnectionString("AzureServiceBus"));
        var sender = serviceBusClient.CreateSender("emailconfirmation");

        var message = new ServiceBusMessage(JsonSerializer.Serialize(request));
        await sender.SendMessageAsync(message);

        return new EmailConfirmationResponse() { Message = "Email sent" ,Succeeded = true };
    }

    public async Task<CodeConfirmationRespone> ConfirmCodeAsync(string email, string code)
    {
        var request = new CodeConfirmationRequest() { Email = email, Code = code };
        return await _client.ConfirmCodeAsync(request);

    }
}
