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

		/// <summary>
		/// Retrieves Classes from the database.
		/// </summary>
		/// <returns>Returns all classes from the database.</returns>
        [HttpGet(Name = "GetClasses")]
        public async Task<ActionResult<IEnumerable<Class>>> Get()
        {
            try
            {
                var classes = await _context.Class.Include(c => c.Teacher).ThenInclude(t => t.Department).Include(c => c.Classroom).ToListAsync();
				
				if (classes.Count == 0)
				{
					return NoContent();
				}
								
                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve classes from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        /// <summary>
        /// Retrieves classes from the database filtered by teacher ID.
        /// </summary>
        /// <param name="teacherId">The ID of the teacher to filter classes by.</param>
        /// <returns>Returns classes filtered by the specified teacher ID.</returns>
        [HttpGet("FilterByTeacherId/{teacherId}")]
        [ProducesResponseType(typeof(IEnumerable<Class>), 200)]
        public async Task<ActionResult<IEnumerable<Class>>> FilterByTeacherId(int teacherId)
        {
            try
            {
                var classes = await _context.Class
                    .Where(c => c.TeacherId == teacherId)
                    .Include(c => c.Teacher)
                    .ThenInclude(t => t.Department)
                    .Include(c => c.Classroom)
                    .ToListAsync();

                if (classes.Count == 0)
                {
                    return NoContent();
                }

                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to filter classes by teacher ID: {teacherId}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves classes from the database filtered by classroom ID.
        /// </summary>
        /// <param name="classroomId">The ID of the classroom to filter classes by.</param>
        /// <returns>Returns classes filtered by the specified classroom ID.</returns>
        [HttpGet("FilterByClassroomId/{classroomId}")]
        [ProducesResponseType(typeof(IEnumerable<Class>), 200)]
        public async Task<ActionResult<IEnumerable<Class>>> FilterByClassroomId(int classroomId)
        {
            try
            {
                var classes = await _context.Class
                    .Where(c => c.ClassroomId == classroomId)
                    .Include(c => c.Teacher)
                    .ThenInclude(t => t.Department)
                    .Include(c => c.Classroom)
                    .ToListAsync();

                if (classes.Count == 0)
                {
                    return NoContent();
                }

                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to filter classes by classroom ID: {classroomId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
