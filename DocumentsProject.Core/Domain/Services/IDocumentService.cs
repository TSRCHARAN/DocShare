using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Domain.Services
{
    public interface IDocumentService
    {
        public Task<Doc?> GetDocumentByIdAsync(string docId);
        public Task<ServiceResult> CreateDocumentAsync(Doc documentDetails, string userEmail);
        public Task<ServiceResult> DeleteUserDocumentByID(string docId, string userEmail);
    }
}
