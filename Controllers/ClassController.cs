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
        /// Retrieves classes from the database, including their associated periods.
        /// </summary>
        /// <returns>Returns all classes with their associated periods.</returns>
        [HttpGet(Name = "GetClasses")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Class>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<Class>>> Get()
        {
            try
            {
                var classes = await _context.Class
                    .Include(c => c.Teacher)
                        .ThenInclude(t => t.Department)
                    .ToListAsync();

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
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<Class>>> FilterByTeacherId(int teacherId)
        {
            try
            {
                var classes = await _context.Class
                    .Where(c => c.TeacherId == teacherId)
                    .Include(c => c.Teacher)
                        .ThenInclude(t => t.Department)
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
        /// Retrieves classes from the database filtered by period contained in the period field.
        /// </summary>
        /// <param name="period">The period to filter classes by.</param>
        /// <returns>Returns classes filtered by the specified period.</returns>
        [HttpGet("FilterByPeriod/{period}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Class>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<Class>>> FilterByPeriod(string period)
        {
            try
            {
                var classes = await _context.Class
                    .Where(c => EF.Functions.Like(c.Period, $"%{period}%"))
                    .Include(c => c.Teacher)
                        .ThenInclude(t => t.Department)
                    .ToListAsync();

                if (classes.Count == 0)
                {
                    return NoContent();
                }

                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to filter classes by period: {period}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Retrieves classes from the database filtered by classroom contained in the period field.
        /// </summary>
        /// <param name="classroom">The classroom to filter classes by.</param>
        /// <returns>Returns classes filtered by the specified classroom.</returns>
        [HttpGet("FilterByClassroom/{classroom}")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<Class>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<Class>>> FilterByClassroom(string classroom)
        {
            try
            {
                var classes = await _context.Class
                    .Where(c => EF.Functions.Like(c.Period, $"%({classroom})%"))
                    .Include(c => c.Teacher)
                        .ThenInclude(t => t.Department)
                    .ToListAsync();

                if (classes.Count == 0)
                {
                    return NoContent();
                }

                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to filter classes by classroom: {classroom}");
                return StatusCode(500, "Internal server error");
            }
        }
        
        /// <summary>
        /// Adds a new class to the database.
        /// </summary>
        /// <param name="newClass">The class object to add.</param>
        /// <returns>Returns the added class.</returns>
        [HttpPost("AddClass")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Class), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Class>> AddClass(Class newClass)
        {
            try
            {
                _context.Class.Add(newClass);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = newClass.Id }, newClass);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add class.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Edits an existing class in the database.
        /// </summary>
        /// <param name="id">The ID of the class to edit.</param>
        /// <param name="updatedClass">The updated class object.</param>
        /// <returns>Returns the edited class.</returns>
        [HttpPut("EditClass/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Class), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Class>> EditClass(int id, Class updatedClass)
        {
            try
            {
                var existingClass = await _context.Class.FindAsync(id);
                if (existingClass == null)
                {
                    return NotFound();
                }

                existingClass.Name = updatedClass.Name;
                existingClass.Code = updatedClass.Code;
                existingClass.TeacherId = updatedClass.TeacherId;
                existingClass.Period = updatedClass.Period;

                await _context.SaveChangesAsync();

                return Ok(existingClass);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to edit class with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
