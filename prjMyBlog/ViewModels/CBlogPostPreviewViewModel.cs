using prjMyBlog.Models;

namespace prjMyBlog.ViewModels
{
    public class CBlogPostPreviewViewModel
    {
        public TBlogPost Post { get; set; }
        public string? PreviewImagePath { get; set; }
        public string CategoryName { get; set; }
        public string AuthorName { get; set; }
    }
}
