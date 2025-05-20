using System;
using System.Collections.Generic;

namespace DocumentsProject.Core.Domain.Models;

public partial class Doc
{
    public string DocId { get; set; } = null!;

    public byte[] Document { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string DocumentCreatedBy { get; set; } = null!;

    public DateTime DocumentUploadedTime { get; set; }
}
