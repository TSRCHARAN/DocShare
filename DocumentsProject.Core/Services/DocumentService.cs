using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using DocumentsProject.Core.Domain.Services.Communication;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsProject.Core.Services
{
    public class DocumentService: IDocumentService
    {
        private readonly DocumentdbContext _documentdbContext;
        private readonly IRedisCacheService _redisCacheService;
        public DocumentService(DocumentdbContext DocumentdbContext, IRedisCacheService redisCacheService)
        {
            _documentdbContext = DocumentdbContext;
            _redisCacheService = redisCacheService;
        }
        public async Task<Doc?> GetDocumentByIdAsync(string docId)
        {
            var dataInMemory = await _redisCacheService.GetCacheAsync(docId);
            if (dataInMemory != null)
            {
                var data = JsonConvert.DeserializeObject<Doc>(dataInMemory.ToString());
                return data;
            }
            return await _documentdbContext.Docs.FirstOrDefaultAsync(d => d.DocId == docId);
        }

        public async Task<ServiceResult> CreateDocumentAsync(Doc documentDetails, string userEmail)
        {
            // Check if the document ID already exists
            var existingDocument = await _documentdbContext.Docs.FirstOrDefaultAsync(d => d.DocId == documentDetails.DocId && d.DocumentCreatedBy == userEmail);

            if (existingDocument != null)
            {
                return new ServiceResult(false, "Document ID already exists.");
            }

            // Create the new document
            Doc newDocument = new Doc
            {
                DocId = documentDetails.DocId,
                Document = documentDetails.Document,
                DocumentName = documentDetails.DocumentName,
                DocumentType = documentDetails.DocumentType,
                DocumentCreatedBy = userEmail,
                DocumentUploadedTime = DateTime.Now
            };

            // Add to database and save
            _documentdbContext.Docs.Add(newDocument);
            await _documentdbContext.SaveChangesAsync();

            var json = JsonConvert.SerializeObject(newDocument);
            await _redisCacheService.SetCacheAsync(documentDetails.DocId,json, TimeSpan.FromMinutes(10));

            return new ServiceResult(true, "Document uploaded successfully.");
        }

        public async Task<ServiceResult> DeleteUserDocumentByID(string docId, string userEmail)
        {
            // Find the document with the specified docId and created by the user
            var document = await _documentdbContext.Docs.FirstOrDefaultAsync(d => d.DocId == docId && d.DocumentCreatedBy == userEmail);

            if (document == null)
            {
                return new ServiceResult(false, "Document not found or you do not have permission to delete it.", null);
            }

            // Remove the document
            _documentdbContext.Docs.Remove(document);

            // Save changes to the database
            var result = await _documentdbContext.SaveChangesAsync();

            if (result > 0)
            {
                return new ServiceResult(true, "Document deleted successfully.", null);
            }
            else
            {
                return new ServiceResult(false, "Failed to delete document.", null);
            }
        }
    }
}
