using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using prjMyBlog.Models;
using prjMyBlog.ViewModels;
using System.Text.Json;

namespace prjMyBlog.Controllers
{
    public class BlogPostController : Controller
    {
        //public IActionResult List(CPostKeywordViewModel vm)
        //{
        //    BlogDbContext db = new BlogDbContext();
        //    string keyword = vm.txtKeyword;
        //    IEnumerable<TBlogPost> datas = null;
        //    if (string.IsNullOrEmpty(keyword))
        //    {
        //        datas = db.TBlogPosts.OrderByDescending(p => p.FCreatedAt);
        //    }
        //    else { 
        //        datas = db.TBlogPosts.Where(p=>p.FTitle.Contains(keyword)||p.FContent.Contains(keyword)).OrderByDescending(p => p.FCreatedAt);
        //    }
        //        return View(datas);
        //}
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult List(CPostKeywordViewModel vm, int page = 1)
        {
            const int pageSize = 10;
            
            // Session 驗證
            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
            {
                // 假如用戶按上一頁但 Session 遺失，就避免進一步執行，立刻導去登入
                return RedirectToAction(CDictionary.LOGIN_ACTION, CDictionary.LOGIN_CONTROLLER);

            }

            var currentUser = JsonSerializer.Deserialize<TUser>(json);

            // 接著再建立 DbContext（避免沒登入還做一堆資料庫動作）
            using var db = new BlogDbContext();

            var query = db.TBlogPosts
                          .Where(p => p.FAuthorId == currentUser.FUserId);

            if (!string.IsNullOrEmpty(vm.txtKeyword))
            {
                query = query.Where(p => p.FTitle.Contains(vm.txtKeyword) || p.FContent.Contains(vm.txtKeyword));
            }

            int totalCount = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var datas = query
                .OrderByDescending(p => p.FCreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Categories = db.TCategories.ToDictionary(c => c.FCategoryId, c => c.FName);
            ViewBag.Authors = db.TUsers.ToDictionary(u => u.FUserId, u => u.FUsername);
            ViewBag.txtKeyword = vm.txtKeyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(datas);
        }



        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Create()
        {
            BlogDbContext db = new BlogDbContext();

            // 從 Session 拿出登入者 JSON
            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction(CDictionary.LOGIN_ACTION, CDictionary.LOGIN_CONTROLLER);
            TUser member = JsonSerializer.Deserialize<TUser>(json);
            ViewBag.AuthorId = member.FUserId;
            ViewBag.Categories = db.TCategories.ToList();
            var tagNames = db.TTags.Select(t => t.FName).ToList();
            ViewBag.TagNamesJson = JsonSerializer.Serialize(tagNames);
            return View();
        }

        [HttpPost]
        public IActionResult Create(TBlogPost post, IFormFile photo, string? newCategoryName)
        {
            BlogDbContext db = new BlogDbContext();

            //  登入者處理（Session）
            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (!string.IsNullOrEmpty(json))
            {
                var user = JsonSerializer.Deserialize<TUser>(json);
                post.FAuthorId = user.FUserId;
            }

            //  分類處理（有 newCategoryName 則新增分類）
            if (!string.IsNullOrWhiteSpace(newCategoryName))
            {
                var exist = db.TCategories.FirstOrDefault(c => c.FName == newCategoryName);
                if (exist != null)
                {
                    post.FCategoryId = exist.FCategoryId;
                }
                else
                {
                    var newCat = new TCategory { FName = newCategoryName };
                    db.TCategories.Add(newCat);
                    db.SaveChanges();
                    post.FCategoryId = newCat.FCategoryId;
                }
            }

            //  設定時間與儲存文章
            post.FCreatedAt = DateTime.Now;
            post.FUpdatedAt = DateTime.Now;
            db.TBlogPosts.Add(post);
            db.SaveChanges(); // 儲存文章才能拿到 FPostId
            string newTagsJson = Request.Form["NewTags"];
            if (!string.IsNullOrWhiteSpace(newTagsJson))
            {
                try
                {
                    var tagItems = JsonSerializer.Deserialize<List<TagItem>>(newTagsJson);
                    foreach (var item in tagItems)
                    {
                        string tagName = item.Value?.Trim();
                        if (string.IsNullOrEmpty(tagName)) continue;

                        var tag = db.TTags.FirstOrDefault(t => t.FName == tagName);
                        if (tag == null)
                        {
                            tag = new TTag { FName = tagName };
                            db.TTags.Add(tag);
                            db.SaveChanges();
                        }

                        db.TPostTags.Add(new TPostTag
                        {
                            FPostId = post.FPostId,
                            FTagId = tag.FTagId
                        });
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "標籤格式錯誤，請重新輸入！";
                    return RedirectToAction("Create");
                }
            }


            db.SaveChanges();

            //  儲存單張圖片
            if (photo != null && photo.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                db.TPostImages.Add(new TPostImage
                {
                    FPostId = post.FPostId,
                    FImagePath = fileName,
                    FUploadedAt = DateTime.Now
                });
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }



        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Details(int? id)
        {
            BlogDbContext db = new BlogDbContext();
            var post = db.TBlogPosts.FirstOrDefault(p => p.FPostId == id);
            if (post == null)
                return NotFound();

            
            var category = db.TCategories.FirstOrDefault(c => c.FCategoryId == post.FCategoryId);
            ViewBag.CategoryName = category?.FName ?? "未分類";
            var author = db.TUsers.FirstOrDefault(u => u.FUserId == post.FAuthorId);
            ViewBag.AuthorName = author?.FUsername ?? "未知作者";

            //  標籤名稱
            var tagNames = (
                from pt in db.TPostTags
                join t in db.TTags on pt.FTagId equals t.FTagId
                where pt.FPostId == post.FPostId
                select t.FName
            ).ToList();
            ViewBag.TagNames = tagNames;

            // 圖片
            var images = db.TPostImages.Where(img => img.FPostId == post.FPostId).ToList();
            ViewBag.Images = images;

            //留言
            var comments = (
                from c in db.TComments
                join u in db.TUsers on c.FUserId equals u.FUserId
                where c.FPostId == post.FPostId
                orderby c.FCreatedAt descending
                select new CCommentViewModel
                {
                    FCommentId = c.FCommentId,
                    Username = u.FUsername,
                    Content = c.FContent,
                    Time = c.FCreatedAt
                }).ToList();

            ViewBag.Comments = comments;

            return View(post);
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpPost]
        public JsonResult AddComment(int postId, string content)
        {
            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return Json(new { success = false, message = "請先登入才能留言。" });

            var user = JsonSerializer.Deserialize<TUser>(json);
            using var db = new BlogDbContext();

            var comment = new TComment
            {
                FPostId = postId,
                FUserId = user.FUserId,
                FContent = content,
                FCreatedAt = DateTime.Now
            };

            db.TComments.Add(comment);
            db.SaveChanges();

            return Json(new
            {
                success = true,
                commentId = comment.FCommentId,
                username = user.FUsername,
                time = comment.FCreatedAt.Value.ToString("yyyy-MM-dd HH:mm"),
                content = comment.FContent,
                canDelete = true // 新增留言的人可刪
            });
        }



        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpPost]
        public JsonResult DeleteComment(int id)
        {
            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return Json(new { success = false, message = "請先登入。" });

            var user = JsonSerializer.Deserialize<TUser>(json);
            using var db = new BlogDbContext();

            var comment = db.TComments.FirstOrDefault(c => c.FCommentId == id);
            if (comment == null)
                return Json(new { success = false, message = "留言不存在。" });

            if (comment.FUserId != user.FUserId && user.FIsAdmin == false)
                return Json(new { success = false, message = "你沒有刪除權限。" });

            db.TComments.Remove(comment);
            db.SaveChanges();

            return Json(new { success = true });
        }




        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpPost]
        public IActionResult UploadImage(IFormFile upload)
        {
            try
            {
                if (upload == null || upload.Length == 0)
                {
                    return Json(new { uploaded = false, error = new { message = "圖片為空" } });
                }

                string imageUrl = PostFactory.SaveImage(upload);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return Json(new { uploaded = false, error = new { message = "無法儲存圖片" } });
                }

                return Json(new { uploaded = true, url = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { uploaded = false, error = new { message = "伺服器錯誤" } });
            }
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            BlogDbContext db = new BlogDbContext();
            var post = db.TBlogPosts.FirstOrDefault(p => p.FPostId == id);
            if (post == null)
                return RedirectToAction("List");


            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json)) return RedirectToAction(CDictionary.LOGIN_ACTION, CDictionary.LOGIN_CONTROLLER);

            var user = JsonSerializer.Deserialize<TUser>(json);
            if (post.FAuthorId != user.FUserId)
            {
                return RedirectToAction("List");
            }
            //  組 ViewModel
            var vm = new CPostEditViewModel
            {
                FPostId = post.FPostId,
                FTitle = post.FTitle,
                FContent = post.FContent,
                FCategoryId = post.FCategoryId
            };
            //  ViewBag 傳入必要資料
            ViewBag.Categories = db.TCategories.ToList();
            ViewBag.Images = db.TPostImages
                .Where(x => x.FPostId == post.FPostId)
                .OrderBy(x => x.FSortOrder)
                .ToList();

            // 補上白名單 JSON（Tagify 使用）
            var allTags = db.TTags.Select(t => t.FName).ToList();
            ViewBag.TagNamesJson = JsonSerializer.Serialize(allTags);
            //  補上已選標籤（Tagify 預設值）
            var selectedTags = (
                from pt in db.TPostTags
                join t in db.TTags on pt.FTagId equals t.FTagId
                where pt.FPostId == post.FPostId
                select new { value = t.FName }
            ).ToList();
            ViewBag.SelectedTagString = selectedTags;

            return View(vm);
        }


        [HttpPost]
        public IActionResult Edit(CPostEditViewModel vm, IFormFile photo)
        {
            BlogDbContext db = new BlogDbContext();
            var post = db.TBlogPosts.FirstOrDefault(p => p.FPostId == vm.FPostId);
            if (post == null) return RedirectToAction("List");


            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction(CDictionary.LOGIN_ACTION, CDictionary.LOGIN_CONTROLLER);

            var currentUser = JsonSerializer.Deserialize<TUser>(json);
            if (post.FAuthorId != currentUser.FUserId) return Forbid();
            // 更新分類
            if (!string.IsNullOrWhiteSpace(vm.NewCategoryName))
            {
                var exist = db.TCategories.FirstOrDefault(c => c.FName == vm.NewCategoryName);
                if (exist != null)
                    post.FCategoryId = exist.FCategoryId;
                else
                {
                    var newCat = new TCategory { FName = vm.NewCategoryName };
                    db.TCategories.Add(newCat);
                    db.SaveChanges();
                    post.FCategoryId = newCat.FCategoryId;
                }
            }
            else
            {
                post.FCategoryId = vm.FCategoryId;
            }

            // 更新內容
            post.FTitle = vm.FTitle;
            post.FContent = vm.FContent;
            post.FUpdatedAt = DateTime.Now;

            //  更新圖片（先刪舊圖）
            if (photo != null && photo.Length > 0)
            {
                var oldImg = db.TPostImages.FirstOrDefault(p => p.FPostId == post.FPostId);
                if (oldImg != null)
                {
                    string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", oldImg.FImagePath);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    db.TPostImages.Remove(oldImg);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                db.TPostImages.Add(new TPostImage
                {
                    FPostId = post.FPostId,
                    FImagePath = fileName,
                    FUploadedAt = DateTime.Now
                });
            }
            //更新標籤：先清空舊的，再新增選取 + 新增輸入的標籤
            var oldTags = db.TPostTags.Where(pt => pt.FPostId == post.FPostId);
            db.TPostTags.RemoveRange(oldTags);
            if (Request.Form["SelectedTagIds"].Count > 0)
            {
                var selectedIds = Request.Form["SelectedTagIds"].Select(int.Parse).ToList();
                foreach (var tagId in selectedIds)
                {
                    db.TPostTags.Add(new TPostTag { FPostId = post.FPostId, FTagId = tagId });
                }
            }

            string newTagsJson = Request.Form["NewTags"];
            if (!string.IsNullOrWhiteSpace(newTagsJson))
            {
                try
                {
                    var tagItems = JsonSerializer.Deserialize<List<TagItem>>(newTagsJson);
                    foreach (var item in tagItems)
                    {
                        string tagName = item.Value?.Trim();
                        if (string.IsNullOrEmpty(tagName)) continue;

                        var tag = db.TTags.FirstOrDefault(t => t.FName == tagName);
                        if (tag == null)
                        {
                            tag = new TTag { FName = tagName };
                            db.TTags.Add(tag);
                            db.SaveChanges();
                        }

                        db.TPostTags.Add(new TPostTag
                        {
                            FPostId = post.FPostId,
                            FTagId = tag.FTagId
                        });
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "新增標籤時發生錯誤，請檢查格式。";
                }
            }
            db.SaveChanges();
            return RedirectToAction("List");
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult DeleteImage(int id, int postId)
        //{
        //    Console.WriteLine($"[DEBUG] DeleteImage called with id={id}, postId={postId}");

        //    PostFactory.DeleteImage(id); // 這會刪圖片+檔案
        //    TempData["message"] = "圖片已成功刪除";

        //    return RedirectToAction("Edit", new { id = postId });
        //}




        public IActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            BlogDbContext db = new BlogDbContext();
            var post = db.TBlogPosts.FirstOrDefault(p => p.FPostId == id);

            if (post == null)
                return RedirectToAction("List");


            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction(CDictionary.LOGIN_ACTION, CDictionary.LOGIN_CONTROLLER);


            var user = JsonSerializer.Deserialize<TUser>(json);
            if (post.FAuthorId != user.FUserId)
                return Forbid(); // 不是作者就禁止刪除

            //  先刪圖片檔與資料庫紀錄
            var images = db.TPostImages.Where(i => i.FPostId == post.FPostId).ToList();
            foreach (var img in images)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", img.FImagePath ?? "");
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                db.TPostImages.Remove(img);
            }
            //  再刪文章本體
            db.TBlogPosts.Remove(post);
            db.SaveChanges();

            return RedirectToAction("List");
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult TagPosts(string tag)
        {
            BlogDbContext db = new BlogDbContext();

            var tagEntity = db.TTags.FirstOrDefault(t => t.FName == tag);
            if (tagEntity == null)
                return NotFound("找不到該標籤");

            var postIds = db.TPostTags
                .Where(pt => pt.FTagId == tagEntity.FTagId)
                .Select(pt => pt.FPostId)
                .Distinct()
                .ToList();

            var posts = db.TBlogPosts
                .Where(p => postIds.Contains(p.FPostId))
                .OrderByDescending(p => p.FCreatedAt)
                .ToList();

            // 額外補上作者、分類、圖片
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
            ViewBag.CurrentTag = tag;
            return View("TagPosts", previews); // 用新檢視頁呈現
        }
    }
}
