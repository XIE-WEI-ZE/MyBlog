using System;
using System.Collections.Generic;

namespace prjMyBlog.Models;

public partial class TPostImage
{
    public int FImageId { get; set; }

    public int? FPostId { get; set; }

    public string? FImagePath { get; set; }

    public DateTime? FUploadedAt { get; set; }

    public int? FSortOrder { get; set; }
}
