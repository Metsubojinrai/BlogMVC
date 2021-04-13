using Blog.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Data
{
    public static class SeedDataRoles
    {
        public static void SeedRoles(RoleManager<Role> roleManager)
        {
            var role1 = new Role
            {
                Name = "Admin",
                Description = "Administrator",
                NormalizedName = "ADMIN"
            };

            var role2 = new Role
            {
                Name = "Editor",
                Description = "Biên tập viên",
                NormalizedName = "EDITOR"
            };

            var role3 = new Role
            {

                Name = "User",
                Description = "Người dùng",
                NormalizedName = "USER"
            };

            List<Role> roleList = new()
            {
                 role1, role2, role3
            };

            foreach(var role in roleList)
            {
                var result = roleManager.RoleExistsAsync(role.Name).Result;
                if (!result)
                    roleManager.CreateAsync(role);
            }
        }
    }
}
