using Blog.Data.Entites;
using Blog.Mcv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Blog.Mcv.Controllers
{
    public class AuthController : Controller
    {
        private IConfiguration _config;
        private DbContext _dbContext;
        public AuthController(DbContext dbContext, IConfiguration config)
        {
            _config = config;
            _dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            var dbSet = _dbContext.Set<UserEntity>();
            var userExists = await dbSet.AnyAsync(u => u.Email == registerViewModel.Email);

            if (userExists)
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(registerViewModel);
            }

            var newUser = new UserEntity
            {
                UserName = registerViewModel.Name,
                Email = registerViewModel.Email,
                Password = registerViewModel.Password, // Plain text password, mirroring existing login logic
                RoleId = 2
            };

            await dbSet.AddAsync(newUser);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var dbSet = _dbContext.Set<UserEntity>();
            var user = await dbSet.Include(x => x.Role).FirstOrDefaultAsync(u => u.Email == loginViewModel.Email && u.Password == loginViewModel.Password);
            if (user is null)
            {
                ViewBag.Error = "Invalid credentails";
                return View();
            }

            //var claims = new List<Claim>
            //{
            //    new Claim(ClaimTypes.Name, user.Nickname),
            //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //};

            //var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            //var principal = new ClaimsPrincipal(identity);

            //var authProperties = new AuthenticationProperties
            //{
            //    IsPersistent = true,
            //};

            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);


            var claims = new List<Claim>
        {
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.Name.ToLowerInvariant())
        };

            var jwtSecret = _config["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT secret is not configured.");
            }
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var tokenoptions = new JwtSecurityToken(
                issuer: "Blog",
                audience: "MVC",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256)
                );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(tokenoptions);

            Response.Cookies.Append("access_token", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Logout()
        {
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Response.Cookies.Delete("access_token");

            return RedirectToAction("Login", "Auth");
        }
    }
}
