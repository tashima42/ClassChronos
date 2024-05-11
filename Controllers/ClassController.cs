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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        
        /// <summary>
        /// Updates the classroom for a class record based on the provided parameters.
        /// </summary>
        /// <param name="classId">The ID of the class record to update.</param>
        /// <param name="classroomId">The ID of the new classroom to assign to the class.</param>
        /// <returns>
        /// Returns a list of classes that have conflicts if any, or a success message if the update is successful.
        /// </returns>
        [HttpPut("UpdateClassroom/{classId}/Classroom/{classroomID}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Class>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<Class>>> UpdateClassroom(int classId, int classroomId)
        {
            try
            {
                
                var classToUpdate = await _context.Class.Include(c => c.Classroom).FirstOrDefaultAsync(c => c.Id == classId);

                if (classToUpdate == null)
                {
                    return NotFound();
                }

                
                var conflicts = await _context.Class
								.Where(c => c.ClassroomId == classroomId && c.Period.Split(new char[] { ',' }).Intersect(classToUpdate.Period.Split(new char[] { ',' })).Any())
								.ToListAsync();


                if (conflicts.Any())
                {
                    return Ok(conflicts);
                }

                
                classToUpdate.ClassroomId = classroomId;
                await _context.SaveChangesAsync();

                return Ok("Classroom updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update classroom for class with ID: {classId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
