using WebApp.Protos;

namespace WebApp.Services;

public interface IVerificationService
{

}
public class VerificationService(EmailConfirmation.EmailConfirmationClient client)
{
    private readonly EmailConfirmation.EmailConfirmationClient _client = client;

    public async Task<EmailConfirmationResponse> SendEmailAsync(string email)
    {
        var request = new EmailConfirmationRequest() { Email = email };
        return await _client.SendEmailAsync(request);
    }

    public async Task<CodeConfirmationRespone> ConfirmCodeAsync(string email, string code)
    {
        var request = new CodeConfirmationRequest() { Email = email, Code = code };
        return await _client.ConfirmCodeAsync(request);
        
    }
}
