using InsightHub.Models;
using InsightHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace InsightHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context; 
            _configuration = configuration;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return BadRequest("Email already exists");

            // Validate selected jobs before creating the user to avoid partial saves.
            var duplicateJobIds = model.SelectedJobs
                .GroupBy(s => s.JobId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateJobIds.Any())
                return BadRequest("Duplicate jobs are not allowed in SelectedJobs.");

            var selectedJobIds = model.SelectedJobs.Select(s => s.JobId).ToList();

            var validJobIds = await _context.Jobs
                .Where(j => selectedJobIds.Contains(j.Id))
                .Select(j => j.Id)
                .ToListAsync();

            if (validJobIds.Count != selectedJobIds.Count)
                return BadRequest("One or more selected jobs are invalid.");

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                BirthDate = model.BirthDate,
                Collage = model.Collage,
                IsGraduated = model.IsGraduated
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (model.SelectedJobs.Count > 0)
            {
                foreach (var selected in model.SelectedJobs)
                {
                    _context.UserJobs.Add(new UserJob
                    {
                        UserId = user.Id,
                        JobId = selected.JobId,
                        YearsExperience = selected.YearsExperience
                    });
                }
            }
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new { 
                token = token,
                expiration = DateTime.UtcNow.AddDays(1),
                message = "User registered successfully" 
            });
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email)
                ?? await _userManager.FindByNameAsync(model.Email);
            if (user == null) return Unauthorized("Invalid username or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded) return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);

            return Ok(new {
                token = token,
                expiration = DateTime.UtcNow.AddDays(1),
                username = user.UserName ?? user.Email,
                firstName = user.FirstName
            });
        }

        // ================= LOGOUT =================
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logged out successfully");
        }

        // ================= Email Exists =================
        [HttpGet("EmailExistance")]
        public async Task<IActionResult> EmailExistance([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("This email doesn't exist.");
            return Ok("Account exists");
        }

        // ================= Profile =================
        [Authorize]
        [HttpPost("profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var user = await _userManager.Users
                .Include(u => u.UserJobs)
                .ThenInclude(uj => uj.Job)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();
            return Ok(new
            {
                user.Email,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.Gender,
                user.Collage,
                user.IsGraduated,
                Jobs = user.UserJobs.Select(uj => new
                {
                    uj.Job.JobName,
                    uj.YearsExperience
                })
            });
        }
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
