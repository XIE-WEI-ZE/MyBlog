using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using prjMyBlog.Helpers;
using prjMyBlog.Models;
using prjMyBlog.ViewModels;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace prjMyBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(BlogDbContext context, IConfiguration configuration)
        {

            _context = context;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(CLoginViewModel vm)
        {
            var user = _context.TUsers.FirstOrDefault(u => u.FUsername == vm.txtAccount);

            if (user == null)
            {
                ViewBag.Message = "帳號不存在";
                return View();
            }

            // =====  加鹽驗證 =====
            // 將 string 轉為 byte[]
            byte[] salt = Convert.FromBase64String(user.FPasswordSalt);
            byte[] inputPwdBytes = Encoding.UTF8.GetBytes(vm.txtPassword);
            byte[] combined = salt.Concat(inputPwdBytes).ToArray();

            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(combined);
            string inputHash = Convert.ToBase64String(hash);

            if (user.FPasswordHash != inputHash)
            {
                ViewBag.Message = "密碼錯誤";
                return View();
            }

            // =====  登入成功後寫入 Session =====
            string json = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, json);

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(CRegisterViewModel vm)
        {
            // 檢查帳號是否已存在
            if (_context.TUsers.Any(u => u.FUsername == vm.txtAccount))
            {
                ViewBag.Message = "帳號已存在";
                return View();
            }

            // 檢查密碼與確認密碼是否一致
            if (vm.txtPassword != vm.txtConfirmPassword)
            {
                ViewBag.Message = "兩次密碼輸入不一致";
                return View();
            }

            // 建立 16 bytes 的 Salt
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            // 組合密碼與 Salt → SHA256 雜湊
            byte[] inputPwdBytes = Encoding.UTF8.GetBytes(vm.txtPassword);
            byte[] combined = salt.Concat(inputPwdBytes).ToArray();

            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(combined);

            //  將 byte[] 轉成 Base64 字串儲存
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            // 建立 User 物件
            var user = new TUser
            {
                FUsername = vm.txtAccount,
                FEmail = vm.txtEmail,
                FPasswordSalt = saltBase64,
                FPasswordHash = hashBase64,
                FIsEnabled = true,
                FIsAdmin = false,
                CreatedAt = DateTime.Now
            };

            _context.TUsers.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove(CDictionary.SK_LOGINED_USER);
            return RedirectToAction("Login");
        }
        //  跳轉到 Google 登入頁
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        //  Google 回傳後的處理邏輯
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;

            //  第一步：從 Google 回傳的資料取得基本資訊
            var email = claims?.FirstOrDefault(c => c.Type.Contains("email"))?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
            var googleId = claims?.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            //  第二步：先用 Email 查詢原本是否已經有註冊帳號
            var user = _context.TUsers.FirstOrDefault(u => u.FEmail == email);

            if (user == null)
            {
                //  第一次登入，尚未註冊，直接新增 Google 帳號
                user = new TUser
                {
                    FUsername = name,
                    FEmail = email,
                    FLoginProvider = "Google",
                    FExternalId = googleId,
                    FIsEnabled = true,
                    FIsAdmin = false,
                    CreatedAt = DateTime.Now
                };
                _context.TUsers.Add(user);
            }
            else
            {
                // 原本就有註冊帳號，只是沒綁 Google（第一次用 Google 登入）
                if (string.IsNullOrEmpty(user.FLoginProvider))
                {
                    user.FLoginProvider = "Google";
                    user.FExternalId = googleId;
                }
            }

            _context.SaveChanges();

            // 寫入 Session
            string json = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, json);

            return RedirectToAction("Index", "Home");
        }

        // ✅ 跳轉到 LINE 登入頁
        public IActionResult LineLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("LineResponse")
            };
            return Challenge(properties, "Line"); // << 注意這裡用 "Line"
        }

        //  LINE 回傳後的處理邏輯
        public async Task<IActionResult> LineResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;

            // 取得 LINE 傳回的資料
            var email = claims?.FirstOrDefault(c => c.Type.Contains("email"))?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == "name")?.Value;
            var lineId = claims?.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;

            // 優先比對 Email 是否已註冊過
            var user = _context.TUsers.FirstOrDefault(u => u.FEmail == email);

            if (user == null)
            {
                // 首次用 LINE 登入，建立帳號
                user = new TUser
                {
                    FUsername = name ?? "LINE使用者",
                    FEmail = email,
                    FLoginProvider = "Line",
                    FExternalId = lineId,
                    FIsEnabled = true,
                    FIsAdmin = false,
                    CreatedAt = DateTime.Now
                };
                _context.TUsers.Add(user);
            }
            else
            {
                // 若是原本傳統註冊，用 LINE 登入就補上 LINE 資料
                if (string.IsNullOrEmpty(user.FLoginProvider))
                {
                    user.FLoginProvider = "Line";
                    user.FExternalId = lineId;
                }
            }

            _context.SaveChanges();

            // 寫入 Session
            string json = JsonSerializer.Serialize(user);
            HttpContext.Session.SetString(CDictionary.SK_LOGINED_USER, json);

            return RedirectToAction("Index", "Home");
        }



        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(CForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = _context.TUsers.FirstOrDefault(u => u.FEmail == vm.Email && u.FIsEnabled == true);

            if (user == null)
            {
                ViewBag.Message = "查無此 Email 或帳號未啟用";
                return View(vm);
            }

            // 從設定檔取得 secretKey
            string secretKey = _configuration["Token:SecretKey"];

            string token = TokenUtility.GenerateToken(user.FEmail, secretKey);
            string resetUrl = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);

            try
            {
                string fromEmail = _configuration["EmailSettings:FromEmail"];
                string appPassword = _configuration["EmailSettings:AppPassword"];

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(appPassword))
                {
                    ViewBag.Message = "寄信失敗：信箱設定未正確載入";
                    return View(vm);
                }

                using var client = new SmtpClient("smtp.gmail.com", 587);
                client.Credentials = new NetworkCredential(fromEmail, appPassword);
                client.EnableSsl = true;

                var mail = new MailMessage(fromEmail, user.FEmail);
                mail.Subject = "重設您的 MyBlog 密碼";
                mail.Body = $"請點擊以下連結來重設密碼（10 分鐘內有效）：\n{resetUrl}";
                client.Send(mail);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "寄信失敗：" + ex.Message;
                return View(vm);
            }

            TempData["Message"] = "重設密碼的驗證信已寄出，請至信箱查收。";
            return RedirectToAction("Login");
        }


        public IActionResult ResetPassword(string token)
        {
            string secretKey = _configuration["Token:SecretKey"];
            if (!TokenUtility.ValidateToken(token, secretKey, out string email))
            {
                return Content("無效或過期的驗證連結");
            }

            var vm = new CResetPasswordViewModel
            {
                Token = token
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult ResetPassword(CResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string secretKey = _configuration["Token:SecretKey"];
            if (!TokenUtility.ValidateToken(vm.Token, secretKey, out string email))
            {
                ViewBag.Message = "無效或過期的驗證連結";
                return View(vm);
            }

            var user = _context.TUsers.FirstOrDefault(u => u.FEmail == email);
            if (user == null)
                return Content("查無此帳號");

            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            byte[] inputPwdBytes = Encoding.UTF8.GetBytes(vm.NewPassword);
            byte[] combined = salt.Concat(inputPwdBytes).ToArray();

            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(combined);

            user.FPasswordSalt = Convert.ToBase64String(salt);
            user.FPasswordHash = Convert.ToBase64String(hash);
            _context.SaveChanges();

            TempData["Message"] = "密碼已成功重設，請重新登入。";
            return RedirectToAction("Login");
        }
    }
 }
