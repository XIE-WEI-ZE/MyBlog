using System;
using System.Collections.Generic;

namespace prjMyBlog.Models;

public partial class TUser
{
    public int FUserId { get; set; }

    public string? FUsername { get; set; }

    public string? FEmail { get; set; }

    public string? FPasswordHash { get; set; }

    public string? FPasswordSalt { get; set; }

    public string? FLoginProvider { get; set; }

    public string? FExternalId { get; set; }

    public string? FPhotoPath { get; set; }

    public string? FBio { get; set; }

    public bool? FIsEnabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? FIsAdmin { get; set; }
}
