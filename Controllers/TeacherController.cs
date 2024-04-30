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
    public class TeacherController : ControllerBase
    {
        private readonly ILogger<TeacherController> _logger;
        private readonly DataContext _context;

        public TeacherController(ILogger<TeacherController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetTeachers")]
        public async Task<ActionResult<IEnumerable<Teacher>>> Get()
        {
            try
            {
                var teachers = await _context.Teachers.ToListAsync();
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve teachers from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
