using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FoodX.Admin.Data;
using FoodX.Admin.Services;

namespace FoodX.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly TestUserService _testUserService;
        private readonly IWebHostEnvironment _environment;

        public TestController(TestUserService testUserService, IWebHostEnvironment environment)
        {
            _testUserService = testUserService;
            _environment = environment;
        }

        /// <summary>
        /// Reset passwords for all test users (Development only)
        /// </summary>
        [HttpPost("reset-test-passwords")]
        public async Task<IActionResult> ResetTestPasswords()
        {
            if (!_environment.IsDevelopment())
            {
                return BadRequest("This endpoint is only available in Development environment");
            }

            var success = await _testUserService.ResetTestUserPasswordsAsync();
            
            if (success)
            {
                return Ok(new 
                { 
                    message = "Test user passwords reset successfully",
                    password = "TestPass123!",
                    users = new[]
                    {
                        "superadmin@test.fdx.trading",
                        "admin@test.fdx.trading",
                        "buyer1@test.fdx.trading",
                        "buyer2@test.fdx.trading",
                        "buyer3@test.fdx.trading",
                        "supplier1@test.fdx.trading",
                        "supplier2@test.fdx.trading",
                        "supplier3@test.fdx.trading",
                        "agent1@test.fdx.trading",
                        "expert1@test.fdx.trading"
                    }
                });
            }
            
            return StatusCode(500, "Some passwords could not be reset. Check logs for details.");
        }

        /// <summary>
        /// Get all users grouped by role (Development only)
        /// </summary>
        [HttpGet("users-by-role")]
        public async Task<IActionResult> GetUsersByRole()
        {
            if (!_environment.IsDevelopment())
            {
                return BadRequest("This endpoint is only available in Development environment");
            }

            var usersByRole = await _testUserService.GetUsersByRoleAsync();
            return Ok(usersByRole);
        }

        /// <summary>
        /// Test login for a user (Development only)
        /// </summary>
        [HttpPost("test-login")]
        public async Task<IActionResult> TestLogin([FromBody] TestLoginRequest request)
        {
            if (!_environment.IsDevelopment())
            {
                return BadRequest("This endpoint is only available in Development environment");
            }

            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required");
            }

            var success = await _testUserService.TestLoginAsync(request.Email, request.Password);
            
            if (success)
            {
                return Ok(new { message = $"Login successful for {request.Email}" });
            }
            
            return Unauthorized(new { message = "Invalid email or password" });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new 
            { 
                status = "healthy",
                environment = _environment.EnvironmentName,
                timestamp = DateTime.UtcNow
            });
        }
    }

    public class TestLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}