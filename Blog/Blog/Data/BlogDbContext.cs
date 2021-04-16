using Blog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Data
{
    public class BlogDbContext : IdentityDbContext<User, Role, long>
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

            builder.Entity<Category>(entity => {
                entity.HasIndex(p => p.Slug);
            });
            builder.Entity<PostCategory>().HasKey(p => new { p.PostID, p.CategoryID });

            // SeedData - chèn ngay bốn sản phẩm khi bảng Product được tạo
            builder.Entity<Product>().HasData(
                    new Product()
                    {
                        ProductId = 1,
                        Name = "Đá phong thuỷ tự nhiên",
                        Description = "Số 1 cao 40cm rộng 20cm dày 20cm màu xanh lá cây đậm",
                        Price = 1000000
                    },
                    new Product()
                    {
                        ProductId = 2,
                        Name = "Đèn đá muối hình tròn",
                        Description = "Trang trí trong nhà Chất liệu : • Đá muối",
                        Price = 1500000
                    },
                    new Product()
                    {
                        ProductId = 3,
                        Name = "Tranh sơn mài",
                        Description = "Tranh sơn mài loại nhỏ 15x 15 giá 50.000",
                        Price = 50000
                    },
                    new Product()
                    {
                        ProductId = 4,
                        Name = "Tranh sơn dầu - Ngựa",
                        Description = "Nguyên liệu thể hiện :    Sơn dầu",
                        Price = 450000
                    }
            );
        }

        public DbSet<Category> Categories { set; get; }
        public DbSet<Post> Posts { set; get; }
        public DbSet<PostCategory> PostCategories { set; get; }
        public DbSet<Product> Products { set; get; }
    }
}
