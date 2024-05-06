using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public LoginController(ILogger<LoginController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetLogins")]
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
        /// <returns>1 for successful login, 0 for unsuccessful login.</returns>
        
        [HttpPost("Authenticate")]
        public async Task<int> Authenticate(string username, string password)
        {
            try
            {
                var user = await _context.Login.FirstOrDefaultAsync(u => u.User == username && u.Password == password);
                return user != null ? 1 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to authenticate user.");
                return 0;
            }
        }
    }
}
