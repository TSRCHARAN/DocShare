using DocumentsProject.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Domain.Services
{
    public interface ILoginService
    {
        public Task<ServiceResult> VerifyUser(string useremail, string password);
        public Task<ServiceResult> SignUpUser(string useremail, string username, string password);
        public Task<bool> AddUser(string useremail, string username);
        public Task<ServiceResult> GetUserDetails(string useremail);
    }
}
