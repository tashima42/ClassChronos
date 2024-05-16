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
    public class DepartmentController : ControllerBase
    {
        private readonly ILogger<DepartmentController> _logger;
        private readonly DataContext _context;

        public DepartmentController(ILogger<DepartmentController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
		/// Retrieves all departments from the database.
		/// </summary>
		/// <returns>Returns departments from the database.</returns>
		[HttpGet("GetDepartments")]
		[Authorize]
		[ProducesResponseType(typeof(IEnumerable<Department>), 200)]
        public async Task<ActionResult<IEnumerable<Department>>> Get()
        {
            try
            {
                var departments = await _context.Department.ToListAsync();
                
				if (departments.Count == 0)
				{
					return NoContent();
				}
				
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve departments from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        /// <summary>
        /// Adds a new department to the database.
        /// </summary>
        /// <param name="department">The department object to add.</param>
        /// <returns>Returns the added department.</returns>
        [HttpPost("AddDepartment")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Department), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Department>> AddDepartment(Department department)
        {
            try
            {
                _context.Department.Add(department);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = department.Id }, department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add department.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Edits an existing department in the database.
        /// </summary>
        /// <param name="id">The ID of the department to edit.</param>
        /// <param name="department">The updated department object.</param>
        /// <returns>Returns the edited department.</returns>
        [HttpPut("EditDepartment/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Department), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Department>> EditDepartment(int id, Department department)
        {
            try
            {
                var existingDepartment = await _context.Department.FindAsync(id);
                if (existingDepartment == null)
                {
                    return NotFound();
                }

                existingDepartment.Name = department.Name;
                await _context.SaveChangesAsync();

                return Ok(existingDepartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to edit department with ID: {id}");
                return StatusCode(500, "Internal server error");
            }
        }
        
    }
}
