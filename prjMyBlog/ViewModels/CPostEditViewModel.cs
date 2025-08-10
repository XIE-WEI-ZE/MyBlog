namespace prjMyBlog.ViewModels
{
    public class CPostEditViewModel
    {
        public int FPostId { get; set; }
        public string? FTitle { get; set; }
        public string? FContent { get; set; }
        public int? FCategoryId { get; set; }
        public string? NewCategoryName { get; set; }
        //public List<IFormFile>? Photos { get; set; } 支持上傳多張照片才用到她
        //public List<int>? SelectedTagIds { get; set; }
        //public string? NewTag { get; set; }



    }
}
