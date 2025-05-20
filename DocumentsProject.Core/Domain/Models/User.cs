using System;
using System.Collections.Generic;

namespace DocumentsProject.Core.Domain.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserEmail { get; set; }

    public string? UserPassword { get; set; }

    public string? UserName { get; set; }
}
