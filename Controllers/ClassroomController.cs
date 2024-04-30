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
    public class ClassroomController : ControllerBase
    {
        private readonly ILogger<ClassroomController> _logger;
        private readonly DataContext _context; // Injecting your DataContext

        public ClassroomController(ILogger<ClassroomController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetClassrooms")]
        public async Task<ActionResult<IEnumerable<Classroom>>> Get()
        {
            try
            {
                var classrooms = await _context.Classrooms.ToListAsync();
                return Ok(classrooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve classrooms from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
