using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tekhnologia.Models;

namespace Tekhnologia.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Login API for Blazor
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();
                return BadRequest(new { errors = allErrors });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { errors = new[] { "Invalid email or password." } });
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok();
        }

        // Logout route 
        [HttpPost("logout")]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromQuery] string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            
            // If there's a returnUrl, redirect to it, otherwise go to home
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return Redirect("/");
        }

        // Register endpoint 
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();
                return BadRequest(new { errors = allErrors });
            }

            var user = new User
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User registered successfully" });
        }

        // Auto-login endpoint after registration (GET to allow cookie write)
        [HttpGet("autologin")]
        public async Task<IActionResult> AutoLogin([FromQuery] string email, [FromQuery] string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return Redirect("/signin");

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect("/");
            }

            return Redirect("/signin");
        }
    }
}
