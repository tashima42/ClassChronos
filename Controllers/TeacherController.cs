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

		/// <summary>
        /// Retrieves all teachers list.
        /// </summary>
        [HttpGet(Name = "GetTeachers")]
        public async Task<ActionResult<IEnumerable<Teacher>>> Get()
        {
            try
            {
                var teachers = await _context.Teacher.Include(t => t.Department).ToListAsync();
                
				if (teachers.Count == 0)
				{
					return NoContent();
				}
				
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve teachers from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        /// <summary>
        /// Retrieves teachers filtered by department ID.
        /// </summary>
        /// <param name="departmentId">The ID of the department to filter by.</param>
        [HttpGet("FilterByDepartment/{departmentId}", Name = "GetTeachersByDepartment")]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachersByDepartment(int departmentId)
        {
            try
            {
                var teachers = await _context.Teacher.Include(t => t.Department)
                                                     .Where(t => t.DepartmentId == departmentId)
                                                     .ToListAsync();

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
