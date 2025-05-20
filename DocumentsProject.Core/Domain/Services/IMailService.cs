using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Domain.Services
{
    public interface IMailService
    {
        public Task<bool> SendMail(string toEmail, string forWhat);
    }
}
