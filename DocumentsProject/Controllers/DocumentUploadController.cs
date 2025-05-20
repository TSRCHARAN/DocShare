using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using DocumentsProject.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;

namespace DocumentsProject.Web.Controllers
{
    [EnableRateLimiting("fixed")]
    public class DocumentUploadController : Controller
    {
        private readonly DocumentdbContext _db;
        private readonly IDocumentService _documentService;

        public DocumentUploadController(DocumentdbContext db, IDocumentService documentService)
        {
            _db = db;
            _documentService = documentService;
        }

        [HttpGet]
        public IActionResult UploadDoc()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(UploadViewModel model)
        {
            // Validate the incoming model
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data." });
            }

            var userEmail = "test";
            bool isUserLoggedIn = User.Identity.IsAuthenticated;

            // Track uploads for unauthenticated users using session
            if (!isUserLoggedIn)
            {
                // Retrieve or initialize upload count from session
                int uploadCount = HttpContext.Session.GetInt32("UploadCount") ?? 0;

                if (uploadCount >= 5)
                {
                    return Json(new { success = false, message = "Upload limit reached. Please log in to upload more documents." });
                }

                // Increment the upload count
                HttpContext.Session.SetInt32("UploadCount", ++uploadCount);
            }
            else
            {
                userEmail = User.FindFirstValue(ClaimTypes.Email);
            }

            // Check if a document with the provided docId already exists
            var existingDocument = await _documentService.GetDocumentByIdAsync(model.DocId);
            if (existingDocument != null)
            {
                return Json(new { success = false, message = "Document ID already exists." });
            }

            // Handle file upload
            if (model.Document != null && model.Document.Length > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await model.Document.CopyToAsync(ms); // Use async method for file copy
                    byte[] fileBytes = ms.ToArray();

                    // Create new document instance
                    Doc newDocument = new Doc
                    {
                        DocId = model.DocId,
                        Document = fileBytes,
                        DocumentName = model.Document.FileName,
                        DocumentType = model.Document.ContentType,
                        DocumentCreatedBy = userEmail,
                        DocumentUploadedTime = DateTime.Now,
                    };

                    // Call the document service to create a new document
                    var result = await _documentService.CreateDocumentAsync(newDocument, userEmail);

                    if (result.Success)
                    {
                        return Json(new { success = true, message = $"Successfully uploaded file: {model.Document.FileName}" });
                    }
                    else
                    {
                        return Json(new { success = false, message = result.Message });
                    }
                }
            }

            return Json(new { success = false, message = "Document upload failed." });
        }
    }
}
