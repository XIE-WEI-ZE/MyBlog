using Microsoft.AspNetCore.Mvc;
using prjMyBlog.Models;
using prjMyBlog.ViewModels;
using System.Text.Json;

namespace prjMyBlog.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult List()
        {
            BlogDbContext db = new BlogDbContext();

            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Login", "Home");

            var user = JsonSerializer.Deserialize<TUser>(json);
            if (user.FIsAdmin != true)
                return RedirectToAction("Index", "Home");

            var datas = db.TCategories.OrderBy(c => c.FCategoryId).ToList();

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(datas);
        }

        public IActionResult Create()
        {
            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Login", "Home");

            var user = JsonSerializer.Deserialize<TUser>(json);
            if (user.FIsAdmin != true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public IActionResult Create(TCategory cat)
        {
            BlogDbContext db = new BlogDbContext();

            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Login", "Home");

            var user = JsonSerializer.Deserialize<TUser>(json);
            if (user.FIsAdmin != true)
                return RedirectToAction("Index", "Home");

            if (db.TCategories.Any(c => c.FName == cat.FName))
            {
                ViewBag.Message = "分類已存在";
                return View();
            }

            db.TCategories.Add(cat);
            db.SaveChanges();

            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            BlogDbContext db = new BlogDbContext();

            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Login", "Home");

            var user = JsonSerializer.Deserialize<TUser>(json);
            if (user.FIsAdmin != true)
                return RedirectToAction("Index", "Home");

            var cat = db.TCategories.FirstOrDefault(c => c.FCategoryId == id);

            if (cat != null)
            {
                bool isUsed = db.TBlogPosts.Any(p => p.FCategoryId == id);
                if (isUsed)
                {
                    TempData["ErrorMessage"] = "此分類已被文章使用，無法刪除。";
                    return RedirectToAction("List");
                }

                db.TCategories.Remove(cat);
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }
    }
}
