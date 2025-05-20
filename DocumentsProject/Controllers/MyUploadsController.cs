using DocumentsProject.Core.Domain.Models;
using DocumentsProject.Core.Domain.Services;
using DocumentsProject.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DocumentsProject.Web.Controllers
{
    [Authorize]
    public class MyUploadsController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDocumentService _documentService;
        public MyUploadsController(IUserService userService, IDocumentService documentService)
        { 
            _userService = userService;
            _documentService = documentService;
        }
        public async Task<IActionResult> Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var res = await _userService.GetUserDocuments(email);
            var documents = (List<Doc>)res.Resource;

            // Initialize an empty list for the view model
            List<DocumentsViewModel> viewModel = new List<DocumentsViewModel>();
            if (documents == null)
            {
                return View();
            }
            foreach(var doc in documents)
            {
                DocumentsViewModel temp = new DocumentsViewModel();
                temp.DocId = doc.DocId;
                temp.DocumentName = doc.DocumentName;
                temp.DocumentUploadedTime = doc.DocumentUploadedTime;
                viewModel.Add(temp);
            }

            return View(viewModel);
        }

        public async Task<IActionResult> DeleteDocument(string docId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var res = await _documentService.DeleteUserDocumentByID(docId, userEmail);
            if(res.Success == false)
            {
                return Json(new { success = false, message = res.Message });
            }
            return Json(new { success = true, message = res.Message });
        }
    }
}
