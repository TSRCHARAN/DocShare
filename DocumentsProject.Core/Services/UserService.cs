using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using DocumentsProject.Core.Domain.Services.Communication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Services
{
    public class UserService :IUserService
    {
        private readonly DocumentdbContext _documentdbContext;
        public UserService(DocumentdbContext DocumentdbContext)
        {
            _documentdbContext = DocumentdbContext;
        }

        public async Task<bool> CheckIfUserExists(string email)
        {
            var res = await _documentdbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
            if (res == null)
            {
                return false;
            }
            return true;
        }

        public async Task<ServiceResult> GetUserDocuments(string userEmail)
        {

            var res = await _documentdbContext.Docs.Where(d => d.DocumentCreatedBy == userEmail)
                                                   .OrderByDescending(d => d.DocumentUploadedTime).ToListAsync();


            if (res.Count == 0)
            {
                return new ServiceResult(false, "No Data Found", null);
            }
            
            return new ServiceResult(true, "Successfully Got Documents of the user", res);
        }

        public async Task<bool> VerifyOtp(string userEmail, string otp)
        {
            var res = await _documentdbContext.Otptables.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            return res.Otp == otp;
        }
        public async Task<bool> UpdatePassword(string userEmail, string newPassword)
        {
            try
            {
                var res = await _documentdbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
                res.UserPassword = newPassword;
                _documentdbContext.Update(res);
                _documentdbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CheckOldPassword(string userEmail, string oldPassword)
        {
            try
            {
                var res = await _documentdbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);

                if (res == null)
                    return false;

                return (res.UserPassword == oldPassword);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
