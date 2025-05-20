using System.ComponentModel.DataAnnotations;

namespace DocumentsProject.Web.ViewModel
{
    public class UploadViewModel
    {
        [Required]
        public string DocId { get; set; }

        [Required]
        public IFormFile Document { get; set; }
    }
}
