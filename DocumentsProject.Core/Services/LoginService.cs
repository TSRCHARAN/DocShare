using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using DocumentsProject.Core.Domain.Services.Communication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Services
{
    public class LoginService: ILoginService
    {
        private readonly DocumentdbContext _documentdbContext;
        public LoginService(DocumentdbContext DocumentdbContext)
        {
            _documentdbContext = DocumentdbContext;
        }
        public async Task<ServiceResult> VerifyUser(string useremail, string password)
        {
            //var user = _documentdbContext.Users.FirstOrDefault(u => u.UserName == username && u.UserPassword == password);

            var user = await _documentdbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == useremail);
            if (user == null)
            {
                return new ServiceResult(false,"User not found");
            }
            if (user.UserPassword == password)
            {
                return new ServiceResult(true,"Login Successful");
            }
            return new ServiceResult(false, "Invalid Password");
        }

        public async Task<ServiceResult> SignUpUser(string useremail, string username, string password)
        {
            try
            {
                var res = await _documentdbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == useremail);
                if (res != null)
                {
                    return new ServiceResult(false, "Email already exists");
                }

                User newUser = new User()
                {
                    UserEmail = useremail,
                    UserName = username,
                    UserPassword = password
                };
                _documentdbContext.Users.Add(newUser);
                _documentdbContext.SaveChanges();

                return new ServiceResult(true, "Account created successfully, Please Login to proceed");
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, "Failed to create account");
            }
        }

        public async Task<bool> AddUser(string useremail, string username)
        {
            try
            {
                var res = await _documentdbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == useremail);
                if (res != null)
                {
                    return false;
                }

                User newUser = new User()
                {
                    UserEmail = useremail,
                    UserName = username,
                    UserPassword = null
                };
                _documentdbContext.Users.Add(newUser);
                _documentdbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ServiceResult> GetUserDetails(string useremail)
        {
            var user = await _documentdbContext.Users.FirstOrDefaultAsync(u => u.UserEmail == useremail);
            if (user == null)
            {
                return new ServiceResult(false, "User not found");
            }
            return new ServiceResult(true, "User Found" ,user.UserName);
        }
    }
}
