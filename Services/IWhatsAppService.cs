namespace Natillera.Services
{
    public interface IWhatsAppService
    {
        Task SendAsync(string message, string? phone = null);
    }
}
