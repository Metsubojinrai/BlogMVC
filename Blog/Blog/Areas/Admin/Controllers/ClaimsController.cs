using Blog.Areas.Admin.Models;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[controller]/[action]")]
    [Authorize(Roles ="Admin")]
    public class ClaimsController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly BlogDbContext _dbContext;
        public ClaimsController(RoleManager<Role> roleManager, BlogDbContext dbContext)
        {
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();
            var model = new ClaimsViewModel
            {
                role = role,
                claims = await (from c in _dbContext.RoleClaims
                                where c.RoleId == role.Id
                                select new EditClaim()
                                {
                                    Id = c.Id,
                                    ClaimType = c.ClaimType,
                                    ClaimValue = c.ClaimValue
                                }).ToListAsync()
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateClaim(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();
            var model = new ClaimsViewModel
            {
                role = role
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClaim(string id, ClaimsViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                bool isExist = !String.IsNullOrEmpty(id);
                if (!isExist)
                    return NotFound();
                IdentityRoleClaim<long> claim = new()
                {
                    RoleId = Convert.ToInt32(id),
                    ClaimType = model.claim.ClaimType,
                    ClaimValue = model.claim.ClaimValue
                };
                _dbContext.RoleClaims.Add(claim);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = id });
            }

            return View(model);
        }
    }
}
