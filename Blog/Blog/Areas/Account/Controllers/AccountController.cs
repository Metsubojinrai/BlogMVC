using Blog.Areas.Account.Models;
using Blog.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Account.Controllers
{
    [Area("Account")]
    [Route("[controller]/[action]")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager
            ,ILogger<AccountController> logger)
        {
            _logger = logger;
            _signManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if(ModelState.IsValid)
            {
                User user = new()
                {
                    UserName = model.Input.UserName,
                    Email = model.Input.Email
                };
                var result = await _userManager.CreateAsync(user, model.Input.Password);

                if(result.Succeeded)
                {
                    _logger.LogInformation("Đăng ký thành công");
                    await _signManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signManager.SignOutAsync();
            _logger.LogInformation("Đăng xuất thành công");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = "")
        {
            //Xóa cookie bên ngoài hiện có để đảm bảo quy trình đăng nhập
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                //Tìm kiếm user theo username hoặc email
                User user = await _userManager.FindByNameAsync(model.Input.UserNameOrEmail);
                if (user == null)
                    user = await _userManager.FindByEmailAsync(model.Input.UserNameOrEmail);

                if(user == null)
                {
                    ModelState.AddModelError("","Không tồn tại tài khoản");
                    return View(model);
                }
                var rs = await _signManager.PasswordSignInAsync(user, model.Input.Password, model.Input.RememberMe, true);
                if(rs.Succeeded)
                {
                    _logger.LogInformation("Đăng nhập thành công");
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else return RedirectToAction("Index", "Home");
                }

                if(rs.IsLockedOut)
                {
                    _logger.LogInformation("Tài khoản bị tạm khóa");
                    return RedirectToAction("Lockout", "Account");
                }
                else
                {
                    _logger.LogInformation("Không đăng nhập được");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
