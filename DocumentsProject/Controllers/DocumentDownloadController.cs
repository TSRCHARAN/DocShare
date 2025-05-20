using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using DocumentsProject.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Reflection.Metadata;
using static System.Net.WebRequestMethods;

namespace DocumentsProject.Web.Controllers
{
    public class DocumentDownloadController : Controller
    {
        private readonly DocumentdbContext _db;
        private readonly IDocumentService _documentService;
        public DocumentDownloadController(DocumentdbContext db, IDocumentService documentService)
        {
            _db = db;
            _documentService = documentService;
        }
        [HttpGet]
        public IActionResult DownloadDoc()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DownloadDocument([FromBody]string docid)
        {

            var result = await _documentService.GetDocumentByIdAsync(docid);


            //var result = await _db.Docs.FirstOrDefaultAsync(d => d.DocId == docid);

            if (result == null)
            {
                return NotFound($"{docid} Document not found");
            }

            var documentBytes = result.Document;

            if (documentBytes == null || documentBytes.Length == 0)
            {
                return NotFound("Downloaded document is empty or null.");
            }

            var documentName = result.DocumentName;

            if (documentName == null || documentName.Length == 0)
            {
                return NotFound("Document Name is empty or null.");
            }

            var documentType = result.DocumentType;

            if (documentType == null || documentType.Length == 0)
            {
                return NotFound("Document Type is empty or null.");
            }
            //return File(documentBytes, "image/jpeg", $"{docid}.jpeg");
            //return File(documentBytes, documentType, documentName);
            var response = new
            {
                fileData = Convert.ToBase64String(documentBytes),
                fileName = documentName,
                contentType = documentType
            };

            return Ok(response);

        }

    }
}
