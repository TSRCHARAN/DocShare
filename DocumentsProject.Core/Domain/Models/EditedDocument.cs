using System;
using System.Collections.Generic;

namespace DocumentsProject.Core.Domain.Models
{
    public partial class EditedDocument
    {
        public string DocId { get; set; } = null!;
        public string? DocName { get; set; }
        public byte[]? Document { get; set; }
    }
}
