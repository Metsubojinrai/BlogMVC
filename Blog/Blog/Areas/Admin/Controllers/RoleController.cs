using Blog.Areas.Admin.Models;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[controller]/[action]")]
    public class RoleController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        public RoleController(RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public List<IdentityRole> Roles { set; get; }

        [TempData] // Sử dụng Session lưu thông báo
        public string StatusMessage { get; set; }

        public async Task<IActionResult> Index()
        {
            List<Role> Roles = await _roleManager.Roles.ToListAsync();
            return View(Roles);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleViewModel model)
        {
            var role = new Role
            {
                Name = model.Input.Name,
                Description = model.Input.Description
            };
            if(ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                else AddErrors(result);
            }

            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index));
                else AddErrors(result);
            }
            else ModelState.AddModelError("", "Không tìm thấy Role");
            return View("Index", _roleManager.Roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                var model = new RoleViewModel();
                model.Input.Name = role.Name;
                model.Input.Description = role.Description;
            }
            else return NotFound($"Không tìm thấy Role với ID: '{id}'.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(string id, RoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (ModelState.IsValid)
                return View(role);
            if (role != null)
            {
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                    StatusMessage = "Cập nhật Role thành công";
                else AddErrors(result);
            }
            else ModelState.AddModelError("","Không tìm thấy Role");
            return View("Index", _roleManager.Roles);
        }
    }
}
