using System;
using System.Collections.Generic;

namespace DocumentsProject.Core.Domain.Models;

public partial class Otptable
{
    public string UserEmail { get; set; } = null!;

    public string? Otp { get; set; }

    public DateTime? OtpCreatedTime { get; set; }
}
