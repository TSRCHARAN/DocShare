using DocumentsProject.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Domain.Services
{
    public interface IUserService
    {
        public Task<bool> CheckIfUserExists(string email);
        public Task<ServiceResult> GetUserDocuments(string userEmail);
        public Task<bool> VerifyOtp(string userEmail, string otp);
        public Task<bool> UpdatePassword(string userEmail, string newPassword);
        public Task<bool> CheckOldPassword(string userEmail, string oldPassword);
    }
}
