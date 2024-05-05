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
    public class ClassController : ControllerBase
    {
        private readonly ILogger<ClassController> _logger;
        private readonly DataContext _context;

        public ClassController(ILogger<ClassController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetClasses")]
        public async Task<ActionResult<IEnumerable<Class>>> Get()
        {
            try
            {
                var classes = await _context.Class.Include(c => c.Teacher).ThenInclude(t => t.Department).Include(c => c.Classroom).ToListAsync();
								
                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve classes from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
