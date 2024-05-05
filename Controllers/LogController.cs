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
		/// Retrieves logs from the database.
		/// </summary>
		/// <returns>Returns logs from the database.</returns>
        [HttpGet(Name = "GetLogs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<IEnumerable<Log>>> Get()
        {
            try
            {
                var logs = await _context.Log
							.Include(l => l.Login)
							.Include(l => l.Teacher).ThenInclude(t => t.Department)
							.Include(l => l.Class).ThenInclude(c => c.Teacher).ThenInclude(t => t.Department)
							.Include(l => l.Class).ThenInclude(c => c.Classroom)
							.Include(l => l.ClassroomOld)
							.Include(l => l.ClassroomNew)
							.ToListAsync();
							
				// If logs is empty, return a custom response
				if (logs.Count == 0)
				{
					return NoContent();
				}
							
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve logs from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
