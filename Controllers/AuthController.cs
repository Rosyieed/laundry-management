using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaundryManagement.Models.ViewModels;
using LaundryManagement.Data;

namespace LaundryManagement.Controllers
{
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class AuthController(AppDbContext _context) : Controller
    {
        [HttpGet("/auth/login")]
        public IActionResult Login(string? returnUrl = null)
        {
            // Jika user sudah login, arahkan ke Home atau returnUrl
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/auth/login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Cari user berdasarkan username
                var user = await _context.users
                    .FirstOrDefaultAsync(u => u.username == model.Username);

                // Validasi user exist dan password valid
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.password_hash))
                {
                    // Formatting avatar path
                    var rawImagePath = user.image_path ?? "";
                    if (!string.IsNullOrEmpty(rawImagePath) && !rawImagePath.StartsWith("/"))
                    {
                        rawImagePath = "/" + rawImagePath;
                    }

                    // Buat klaim identitas
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                        new Claim(ClaimTypes.Name, user.username),
                        new Claim("FullName", user.name),
                        new Claim(ClaimTypes.Role, user.role),
                        new Claim("AvatarPath", rawImagePath)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
                    };

                    // Sign User in
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Redirect amam
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Username atau Password salah.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Terjadi kesalahan sistem: " + ex.Message);
            }

            return View(model);
        }

        [HttpPost("/auth/logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet("/auth/access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
