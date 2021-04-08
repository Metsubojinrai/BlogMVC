using Blog.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Data
{
    public static class SeedDataAdmin
    {
        public static void SeedAdmin(UserManager<User> userManager)
        {
            if(userManager.FindByEmailAsync("admin@gmail.com").Result == null)
            {
                User user = new User
                {
                    UserName = "admin",
                    Email = "admin@gmail.com"
                };
                var result = userManager.CreateAsync(user, "123456").Result;
                if (result.Succeeded)
                    userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
