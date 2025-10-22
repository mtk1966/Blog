using Blog.Data;
using Blog.Data.Entites;
using Blog.Mcv.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blog.Mcv.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly BlogDbContext _dbContext;

        public AccountController(BlogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Set<UserEntity>()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            var model = new AccountViewModel
            {
                Name = user.UserName,
                Email = user.Email,
                Role = user.Role.Name
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> NewAccountInformation()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Set<UserEntity>().FindAsync(int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            var model = new NewAccountInformationViewModel
            {
                Name = user.UserName,
                Email = user.Email,
                Password = user.Password
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> NewAccountInformation(NewAccountInformationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _dbContext.Set<UserEntity>().FindAsync(int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.Name;
            user.Email = model.Email;
            user.Password = model.Password;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
