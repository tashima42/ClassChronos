using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;
        private readonly DataContext _context; 

        public LogController(ILogger<LogController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

		/// <summary>
		/// Retrieves all logs. Only accessible to users with the 'Admin' role.
		/// </summary>
		/// <returns>Returns a list of logs.</returns>
		[HttpGet("GetLogs")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<IEnumerable<Log>>> GetLogs()
		{
			try
			{
				var logs = await _context.Log
					.Include(l => l.Login)
					.Include(l => l.Teacher)
					.Include(l => l.Class)
					.ToListAsync();

				return Ok(logs);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "Internal server error");
			}
		}
	}
}
