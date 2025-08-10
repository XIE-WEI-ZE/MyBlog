using System;
using System.Collections.Generic;

namespace prjMyBlog.Models;

public partial class TComment
{
    public int FCommentId { get; set; }

    public int? FPostId { get; set; }

    public int? FUserId { get; set; }

    public string? FContent { get; set; }

    public DateTime? FCreatedAt { get; set; }
}
