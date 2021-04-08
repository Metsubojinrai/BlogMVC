using Blog.Areas.Manage.Models;
using Blog.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ManageController> _logger;
        public ManageController(UserManager<User> userManager,
            SignInManager<User> signInManager, ILogger<ManageController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound($"Không tải được tài khoản ID: '{_userManager.GetUserId(User)}'.");
            }

            var model = new UserProfileViewModel()
            {
                UserName = user.UserName,
                Input = new UserProfileViewModel.InputModel
                {
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    Address=user.Address,
                    Birthday=user.Birthday,
                    ProfilePicture=user.ProfilePicture
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không có tài khoản ID :'{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if(model.Input.PhoneNumber!=phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user,model.Input.PhoneNumber);
                if(!setPhoneResult.Succeeded)
                {
                    model.StatusMessage = "Lỗi cập nhật số điện thoại";
                    return RedirectToAction("Index", "Manage");
                }
            }

            if(model.Input.FullName!=user.FullName)
            {
                user.FullName = model.Input.FullName;
                await _userManager.UpdateAsync(user);
            }

            if (model.Input.Address != user.Address)
            {
                user.Address = model.Input.Address;
                await _userManager.UpdateAsync(user);
            }

            if (model.Input.Birthday != user.Birthday)
            {
                user.Birthday = model.Input.Birthday;
                await _userManager.UpdateAsync(user);
            }

            if(Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();
                var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\wwwroot\\File\\Image\\User\\");
                bool basePathExist = System.IO.Directory.Exists(basePath);
                if (!basePathExist)
                    Directory.CreateDirectory(basePath);
                var fileName = Path.Combine(file.FileName);
                var filePath = Path.Combine(basePath, file.FileName);
                if(!System.IO.File.Exists(filePath))
                {
                    using (var stream = new FileStream(filePath,FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }                
                }
                user.ProfilePicture = fileName;
                await _userManager.UpdateAsync(user);
            }
            await _signInManager.RefreshSignInAsync(user);
            model.StatusMessage = "Hồ sơ đã cập nhật";
            return RedirectToAction("Index", "Manage");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user==null)
            {
                return NotFound($"Không có tài khoản ID :'{_userManager.GetUserId(User)}'.");
            }

            var hasPwd = await _userManager.HasPasswordAsync(user);
            if(!hasPwd)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if(user==null)
            {
                return NotFound($"Không có tài khoản ID :'{_userManager.GetUserId(User)}'.");
            }

            var changePwdResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if(!changePwdResult.Succeeded)
            {
                AddErrors(changePwdResult);
                return View(model);
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("Thay đổi mật khẩu thành công.");
            model.StatusMessage = "Mật khẩu đã thay đổi.";
            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Không có tài khoản ID: '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Không có tài khoản ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            model.StatusMessage = "Mật khẩu đã được thiết lập.";

            return RedirectToAction(nameof(SetPassword));
        }
    }
}
