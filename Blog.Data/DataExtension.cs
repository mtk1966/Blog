using Blog.Data.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Data
{
    public static class DataExtension
    {
        public static IServiceCollection AddBlogsSbData(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<DbContext, BlogDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            return services;
        }

        public static async Task EnsureCreatedAndSeedAsync(this DbContext context)
        {
            if (await context.Database.EnsureCreatedAsync())
            {
                // seed
                var UserRole = new RoleEntity
                {
                    Name = "user",
                };
                var ModeratorRole = new RoleEntity
                {
                    Name = "mod",
                };
                var AdminRole = new RoleEntity
                {
                    Name = "admin",
                };
                context.Set<RoleEntity>().AddRange(AdminRole, UserRole, ModeratorRole);
                await context.SaveChangesAsync();
                var user1 = new UserEntity
                {
                    UserName = "mtk",
                    Email = "mtk@gmail.com",
                    Password = "1234",
                    RoleId = AdminRole.Id
                };

                var user2 = new UserEntity
                {
                    UserName = "Mahmut",
                    Email = "mahmut@gmail.com",
                    Password = "1234",
                    RoleId = ModeratorRole.Id
                };

                var user3 = new UserEntity
                {
                    UserName = "Veli",
                    Email = "veli@gmail.com",
                    Password = "1234",
                    RoleId = UserRole.Id
                };

                context.Set<UserEntity>().AddRange(user1, user2, user3);

                await context.SaveChangesAsync();

                var blog1 = new BlogsEntity
                {
                    UserId = user1.Id,
                    Title = "Kayra",
                    Content = "Farazi",
                    CreatedAt = DateTime.Now
                };

                context.Set<BlogsEntity>().Add(blog1);

                await context.SaveChangesAsync();
            }

        }
    }
}