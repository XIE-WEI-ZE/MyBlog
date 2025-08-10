using Microsoft.AspNetCore.Mvc;

namespace prjMyBlog.ViewModels
{
    public class CUserProfileViewModel
    {
        public int FUserId { get; set; }

        public string? FUsername { get; set; }

        public string? FBio { get; set; }

        public IFormFile? PhotoFile { get; set; } // 用來上傳圖片

        public string? ExistingPhotoPath { get; set; } // 用來顯示舊大頭貼
    }
}
