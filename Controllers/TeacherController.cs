using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
        [Authorize]
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
        [Authorize]
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
        
        /// <summary>
        /// Adds a new teacher to the database.
        /// </summary>
        /// <param name="teacher">The teacher object to add.</param>
        /// <returns>Returns the added teacher.</returns>
        [HttpPost("AddTeacher")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Teacher), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Teacher>> AddTeacher(Teacher teacher)
        {
            try
            {
                _context.Teacher.Add(teacher);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = teacher.Id }, teacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add teacher.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Edits an existing teacher in the database.
        /// </summary>
        /// <param name="id">The ID of the teacher to edit.</param>
        /// <param name="teacher">The updated teacher object.</param>
        /// <returns>Returns the edited teacher.</returns>
        [HttpPut("EditTeacher/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Teacher), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Teacher>> EditTeacher(int id, Teacher teacher)
        {
            try
            {
                var existingTeacher = await _context.Teacher.FindAsync(id);
                if (existingTeacher == null)
                {
                    return NotFound();
                }

                existingTeacher.Name = teacher.Name;

                await _context.SaveChangesAsync();

                return Ok(existingTeacher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to edit teacher with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
