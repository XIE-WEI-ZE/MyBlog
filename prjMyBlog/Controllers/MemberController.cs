using Microsoft.AspNetCore.Mvc;
using prjMyBlog.Models;
using prjMyBlog.ViewModels;
using System.Text.Json;

namespace prjMyBlog.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Profile()
        {
            string json = HttpContext.Session.GetString(CDictionary.SK_LOGINED_USER);
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Login", "Account");

            var user = JsonSerializer.Deserialize<TUser>(json);
            using var db = new BlogDbContext();

            var dbUser = db.TUsers.FirstOrDefault(u => u.FUserId == user.FUserId);
            if (dbUser == null) return NotFound();

            var vm = new CUserProfileViewModel
            {
                FUserId = dbUser.FUserId,
                FUsername = dbUser.FUsername,
                FBio = dbUser.FBio,
                ExistingPhotoPath = dbUser.FPhotoPath
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Profile(CUserProfileViewModel vm)
        {
            using var db = new BlogDbContext();
            var user = db.TUsers.FirstOrDefault(u => u.FUserId == vm.FUserId);
            if (user == null) return NotFound();

            // 更新暱稱與個人簡介
            user.FUsername = vm.FUsername;
            user.FBio = vm.FBio;

            // 若有上傳大頭貼
            if (vm.PhotoFile != null && vm.PhotoFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.PhotoFile.FileName);
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                vm.PhotoFile.CopyTo(stream);

                user.FPhotoPath = fileName;
            }

            db.SaveChanges();

            // 更新 Session 內的資料
            string json = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, json);

            ViewBag.Message = "更新成功！";
            return RedirectToAction("Profile");
        }
    }
}
