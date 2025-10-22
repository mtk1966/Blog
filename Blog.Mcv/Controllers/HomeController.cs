using Blog.Data.Entites;
using Blog.Mcv.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq; // Added
using System.Text.Json; // Added

namespace Blog.Mcv.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, DbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var blogs = await _dbContext.Set<BlogsEntity>()
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new BlogTableViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    BlogOwner = b.User.UserName,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
            _logger.LogInformation("user istek yapti");
            return View(blogs);
        }

        public async Task<IActionResult> MyBlogs()
        {
            var userId = GetUserId();
            var blogs = await _dbContext.Set<BlogsEntity>()
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Where(b => b.UserId == userId)
                .Select(b => new BlogTableViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
            return View(blogs);
        }
        public IActionResult AddBlog()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddBlog([FromForm] NewBlogViewModel newBlogVieModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var userId = GetUserId();

            var blogPost = new BlogsEntity
            {
                Title = newBlogVieModel.Title,
                Content = newBlogVieModel.Content,
                UserId = (int)userId,
                CreatedAt = DateTime.Now
            };


            _dbContext.Add(blogPost);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: AddRole
        [PermissionAuthorize("Roles.Create")]
        public IActionResult AddRole()
        {
            var model = new NewRoleViewModel();
            foreach (var permission in AppPermissions.AllPermissions)
            {
                model.Permissions.Add(new PermissionViewModel { Name = permission, IsSelected = false });
            }
            return View(model);
        }

        // POST: AddRole
        [HttpPost]
        [PermissionAuthorize("Roles.Create")]
        public async Task<IActionResult> AddRole(NewRoleViewModel newRoleViewModel) // Removed [FromForm]
        {
            if (!ModelState.IsValid)
            {
                // If model state is invalid, return the view with the current model to show validation errors
                // and retain user's selections.
                return View(newRoleViewModel);
            }

            var newrole = new RoleEntity
            {
                Name = newRoleViewModel.Name
            };

            // Serialize selected permissions to JSON
            var selectedPermissions = newRoleViewModel.Permissions
                                        .Where(p => p.IsSelected)
                                        .Select(p => p.Name)
                                        .ToList();
            newrole.PermissionsJson = JsonSerializer.Serialize(selectedPermissions);

            _dbContext.Add(newrole);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("UserRole");
        }

        // GET: EditRole
        [HttpGet]
        [PermissionAuthorize("Roles.Edit")]
        public async Task<IActionResult> EditRole(int id)
        {
            var role = await _dbContext.Set<RoleEntity>().FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new NewRoleViewModel
            {
                Name = role.Name
            };

            var selectedPermissions = new List<string>();
            if (!string.IsNullOrEmpty(role.PermissionsJson))
            {
                selectedPermissions = JsonSerializer.Deserialize<List<string>>(role.PermissionsJson);
            }

            foreach (var permission in AppPermissions.AllPermissions)
            {
                model.Permissions.Add(new PermissionViewModel
                {
                    Name = permission,
                    IsSelected = selectedPermissions.Contains(permission)
                });
            }

            return View(model);
        }

        // POST: EditRole
        [HttpPost]
        [PermissionAuthorize("Roles.Edit")]
        public async Task<IActionResult> EditRole(int id, NewRoleViewModel newRoleViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(newRoleViewModel);
            }

            var roleToUpdate = await _dbContext.Set<RoleEntity>().FindAsync(id);
            if (roleToUpdate == null)
            {
                return NotFound();
            }

            roleToUpdate.Name = newRoleViewModel.Name;
            var selectedPermissions = newRoleViewModel.Permissions
                                        .Where(p => p.IsSelected)
                                        .Select(p => p.Name)
                                        .ToList();
            roleToUpdate.PermissionsJson = JsonSerializer.Serialize(selectedPermissions);

            _dbContext.Update(roleToUpdate);
            return RedirectToAction("UserRole");
        }

        // POST: DeleteRole
        [HttpPost]
        [PermissionAuthorize("Roles.Delete")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var roleToDelete = await _dbContext.Set<RoleEntity>().FindAsync(id);
            if (roleToDelete == null)
            {
                return NotFound();
            }

            // Find the default 'user' role
            var defaultUserRole = await _dbContext.Set<RoleEntity>().FirstOrDefaultAsync(r => r.Name == "user");

            if (defaultUserRole == null)
            {
                // Handle case where default 'user' role does not exist
                // For now, we'll just return a bad request or an error message
                return BadRequest("Default 'user' role not found. Cannot reassign users.");
            }

            // Reassign users from the role being deleted to the default 'user' role
            var usersInDeletedRole = await _dbContext.Set<UserEntity>()
                                                    .Where(u => u.RoleId == id)
                                                    .ToListAsync();

            foreach (var user in usersInDeletedRole)
            {
                user.RoleId = defaultUserRole.Id;
            }

            _dbContext.Remove(roleToDelete);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("UserRole");
        }

        public async Task<IActionResult> Details([FromRoute] int id)
        {
            var blog = await _dbContext.Set<BlogsEntity>()
                .Include(b => b.User)
                .Include(b => b.Comments)
                .ThenInclude(c => c.User)
                .SingleOrDefaultAsync(b => b.Id == id);

            if (blog is null)
            {
                return NotFound();
            }

            var detailsViewModel = new DetailsViewModel
            {
                Blog = blog,
                Comments = blog.Comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserName = c.User.UserName,
                    CreatedAt = c.CreatedAt,
                    UserId = c.UserId
                }).ToList()
            };

            return View(detailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromForm] int blogId, [FromForm] string content)
        {
            var userId = GetUserId();

            var comment = new CommentEntity
            {
                Content = content,
                BlogId = blogId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _dbContext.Add(comment);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = blogId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId, int blogId)
        {
            var comment = await _dbContext.Set<CommentEntity>().FindAsync(commentId);

            if (comment != null)
            {
                _dbContext.Set<CommentEntity>().Remove(comment);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { id = blogId });
        }

        // GET: EditComment
        [HttpGet]
        public async Task<IActionResult> EditComment(int id)
        {
            var comment = await _dbContext.Set<CommentEntity>().FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var model = new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content
            };

            return View(model);
        }

        // POST: EditComment
        [HttpPost]
        public async Task<IActionResult> EditComment(int id, [FromForm] CommentViewModel commentViewModel)
        {
            var blogId = commentViewModel.Id;
            if (id != commentViewModel.Id)
            {
                return NotFound();
            }

            var commentToUpdate = await _dbContext.Set<CommentEntity>().FindAsync(id);
            if (commentToUpdate == null)
            {
                return NotFound();
            }

            commentToUpdate.Content = commentViewModel.Content;

            _dbContext.Update(commentToUpdate);
            await _dbContext.SaveChangesAsync();

            // Redirect back to the blog detail page
            return RedirectToAction("Detail", new { id = commentToUpdate.BlogId });
        }

        public async Task<IActionResult> DeleteBlog([FromRoute] int id)
        {
            var blog = await _dbContext.Set<BlogsEntity>()
                .SingleOrDefaultAsync(b => b.Id == id);

            if (blog is null)
            {
                return NotFound();
            }

            _dbContext.Remove(blog);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("MyBlogs");
        }
        [Authorize(Roles = "mod,admin")]
        public async Task<IActionResult> DeleteBlogMod([FromRoute] int id)
        {
            var blog = await _dbContext.Set<BlogsEntity>()
                .SingleOrDefaultAsync(b => b.Id == id);

            if (blog is null)
            {
                return NotFound();
            }

            _dbContext.Remove(blog);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("MyBlogs");
        }
        [PermissionAuthorize("Roles.Read")]
        public async Task<IActionResult> UserRole(string searchString)
        {
            var roles = await _dbContext.Set<RoleEntity>()
                .Where(r => r.Name != "admin")
                .ToListAsync();

            ViewBag.Roles = roles;

            var usersQuery = _dbContext.Set<UserEntity>()
                .Include(u => u.Role) // Include the role information
                .Where(u => u.Role.Name != "admin");

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString));
            }

            var users = await usersQuery
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.UserName,
                    Email = u.Email,
                    RoleId = u.RoleId
                })
                .ToListAsync();

            return View(users);
        }
        [HttpPost]
        [PermissionAuthorize("Roles.Edit")]
        public async Task<IActionResult> UpdateUserRole(int userId, int roleId)
        {
            var user = await _dbContext.Set<UserEntity>().FindAsync(userId);
            if (user != null)
            {
                user.RoleId = roleId;
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(UserRole));
        }
        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _dbContext.Set<BlogsEntity>().FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            var model = new EditViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content
            };
            return View(model);
        }

        // POST: Events/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] EditViewModel editViewModel)
        {
            if (id != editViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var blog = await _dbContext.Set<BlogsEntity>().FindAsync(id);
                if (blog == null)
                {
                    return NotFound();
                }

                blog.Title = editViewModel.Title;
                blog.Content = editViewModel.Content;

                _dbContext.Update(blog);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(editViewModel);
        }




        public int GetUserId()
        {
            return int.Parse(User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub) ?? throw new InvalidOperationException());
        }

    }
}