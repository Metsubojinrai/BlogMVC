using Blog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Data
{
    public class BlogDbContext : IdentityDbContext<User, Role, Guid>
    {
        public BlogDbContext(DbContextOptions<BlogDbContext>options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach(var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if(tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName[6..]);
                }
            }
        }
    }
}
