using System;
using System.Collections.Generic;

namespace prjMyBlog.Models;

public partial class TPostTag
{
    public int FPostTagId { get; set; }

    public int? FPostId { get; set; }

    public int? FTagId { get; set; }
}
