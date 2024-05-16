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
    }
}
