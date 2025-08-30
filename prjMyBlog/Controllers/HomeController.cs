using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjMyBlog.Models;
using prjMyBlog.ViewModels;
using System.Diagnostics;

namespace prjMyBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Index(string? txtKeyword, int page = 1)
        {
            const int pageSize = 9; // 每頁顯示筆數
            BlogDbContext db = new BlogDbContext();

            var query = db.TBlogPosts.AsQueryable();

            //  搜尋條件：標題或內文模糊查詢
            if (!string.IsNullOrWhiteSpace(txtKeyword))
            {
                query = query.Where(p => p.FTitle.Contains(txtKeyword) || p.FContent.Contains(txtKeyword));
            }

            //  分頁計算
            int totalCount = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var posts = query
                .OrderByDescending(p => p.FCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var previews = posts.Select(post =>
            {
                var previewImage = db.TPostImages
                    .Where(img => img.FPostId == post.FPostId)
                    .OrderBy(img => img.FSortOrder)
                    .FirstOrDefault();

                var category = db.TCategories.FirstOrDefault(c => c.FCategoryId == post.FCategoryId);
                var author = db.TUsers.FirstOrDefault(u => u.FUserId == post.FAuthorId);

                return new CBlogPostPreviewViewModel
                {
                    Post = post,
                    PreviewImagePath = previewImage?.FImagePath,
                    CategoryName = category?.FName ?? "未分類",
                    AuthorName = author?.FUsername ?? "未知作者"
                };
            }).ToList();

            // 傳 ViewBag 給 Razor 頁面
            ViewBag.txtKeyword = txtKeyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(previews);
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
