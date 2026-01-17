using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Services
{
    public interface IWhatsAppService
    {
        Task SendAsync(string message, string? phone = null);
    }
}
