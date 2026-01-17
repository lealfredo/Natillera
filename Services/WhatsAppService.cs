using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        public async Task SendAsync(string message, string? phone = null)
        {
            var encoded = Uri.EscapeDataString(message);

            string url = phone == null
                ? $"https://wa.me/?text={encoded}"
                : $"https://wa.me/57{phone}?text={encoded}";

            await Launcher.Default.OpenAsync(url);
        }
    }
}
