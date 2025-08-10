using System;
using System.Collections.Generic;

namespace prjMyBlog.Models;

public partial class TBlogPost
{
    public int FPostId { get; set; }

    public string? FTitle { get; set; }

    public string? FContent { get; set; }

    public DateTime? FCreatedAt { get; set; }

    public DateTime? FUpdatedAt { get; set; }

    public int? FCategoryId { get; set; }

    public int? FAuthorId { get; set; }
}
