namespace DocumentsProject.Web.ViewModel
{
    public class DocumentsViewModel
    {
        public string DocId { get; set; } = null!;
        public string DocumentName { get; set; } = null!;
        public DateTime? DocumentUploadedTime { get; set; }
    }
}
