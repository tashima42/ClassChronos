using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UTFClassAPI;

namespace UTFClassAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly DataContext _context;
        private readonly Crypto _crypto;

        public LoginController(ILogger<LoginController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
            _crypto = new Crypto();
        }

        [HttpGet("GetLogins")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Login>>> Get()
        {
            try
            {
                var logins = await _context.Login.ToListAsync();
                return Ok(logins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve logins from the database.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Authenticates a user based on username and password.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>JWT token for successful login, or Unauthorized for unsuccessful login.</returns>
        [HttpPost("Authenticate")]
        public async Task<ActionResult<string>> Authenticate(string username, string password)
        {
            try
            {
                var user = await _context.Login.FirstOrDefaultAsync(u => u.User == username && u.Password == password);
                if (user != null)
                {
                    // User is authenticated, generate JWT token
                    var token = GenerateJwtToken(user);
                    return Ok(token);
                }
                else
                {
                    // User authentication failed
                    return Unauthorized();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate user.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
		/// Creates a new login if the user is admin.
		/// </summary>
		/// <param name="login">The login object containing user details.</param>
		/// <returns>1 if the login is created successfully, or Unauthorized if the user is not an admin.</returns>
		[HttpPost("CreateLogin")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<int>> CreateLogin(Login login)
		{
			try
			{
				_context.Login.Add(login);
				await _context.SaveChangesAsync();
				return Ok(1);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to create login.");
				return StatusCode(500, "Internal server error");
			}
		}

        private string GenerateJwtToken(Login user)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_crypto.GetSecretKeyAsString());
    
			// Include user ID as a claim
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.User),
				new Claim(ClaimTypes.Role, user.IsAdmin == 1 ? "Admin" : "User"),
				new Claim("UserId", user.Id.ToString())
			};

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
    }
}
