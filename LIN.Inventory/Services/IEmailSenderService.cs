namespace LIN.Inventory.Services;

public interface IEmailSenderService
{
    Task<bool> Send(string to, string subject, string body);
}