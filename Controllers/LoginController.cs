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
    }
}
