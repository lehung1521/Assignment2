
using AgentPortal.Web.Models.ViewModels;
using B2BWholesale.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AgentPortal.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị trang đăng nhập
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        // Xử lý đăng nhập
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == model.UserName);

            if (user == null)
            {
                ModelState.AddModelError("",
                    "Invalid username or password.");
                return View(model);
            }

            // Chỉ Agent mới được đăng nhập Web
            if (user.Role != "Agent" || user.AgentID == null)
            {
                ModelState.AddModelError("",
                    "Invalid username or password.");
                return View(model);
            }

            // Kiểm tra tài khoản bị khóa
            if (user.Lock)
            {
                ModelState.AddModelError("",
                    "This account is locked.");
                return View(model);
            }

            // Hash mật khẩu theo dữ liệu demo hiện tại của Quý
            string passwordHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(model.Password)
                )
            );

            bool passwordCorrect =
                CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(passwordHash),
                    Encoding.ASCII.GetBytes(user.Password)
                );

            if (!passwordCorrect)
            {
                user.FailedLoginCount++;

                if (user.FailedLoginCount >= 5)
                    user.Lock = true;

                await _context.SaveChangesAsync();

                ModelState.AddModelError("",
                    "Invalid username or password.");
                return View(model);
            }

            // Đăng nhập đúng: reset số lần sai
            user.FailedLoginCount = 0;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.UserID.ToString()),

                new Claim(ClaimTypes.Name, user.UserName),

                new Claim(ClaimTypes.Role, user.Role),

                new Claim("AgentID", user.AgentID.Value.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "Account");
        }

        // Trang từ chối truy cập
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content("Access denied.");
        }
    }
}
