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
    public class ClassroomController : ControllerBase
    {
        private readonly ILogger<ClassroomController> _logger;
        private readonly DataContext _context;

        public ClassroomController(ILogger<ClassroomController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

		/// <summary>
		/// Retrieves all classrooms from the database.
		/// </summary>
		/// <returns>Returns classrooms from the database.</returns>
        [HttpGet(Name = "GetClassrooms")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Classroom>>> Get()
        {
            try
            {
                var classrooms = await _context.Classroom.ToListAsync();
                
                
				if (classrooms.Count == 0)
				{
					return NoContent();
				}
                
                return Ok(classrooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve classrooms from the database.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        /// <summary>
		/// Retrieves classrooms from the database based on a partial name match.
		/// </summary>
		/// <param name="name">Partial name to match against classroom names.</param>
		/// <returns>Returns classrooms that match the partial name.</returns>
		[HttpGet("SearchByName/{name}")]
		[Authorize]
		[ProducesResponseType(typeof(IEnumerable<Classroom>), 200)]
		public async Task<ActionResult<IEnumerable<Classroom>>> SearchByName(string name)
		{
			try
			{
				var classrooms = await _context.Classroom.Where(c => EF.Functions.Like(c.Name, $"%{name}%")).ToListAsync();

				if (classrooms.Count == 0)
				{
					return NoContent();
				}

				return Ok(classrooms);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to search classrooms by name: {name}");
				return StatusCode(500, "Internal server error");
			}
		}
		
		/// <summary>
/// Adds a new classroom to the database.
/// </summary>
/// <param name="classroom">The classroom object to add.</param>
/// <returns>Returns the added classroom.</returns>
[HttpPost("AddClassroom")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(Classroom), 201)]
[ProducesResponseType(400)]
[ProducesResponseType(500)]
public async Task<ActionResult<Classroom>> AddClassroom(Classroom classroom)
{
    try
    {
        _context.Classroom.Add(classroom);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = classroom.Id }, classroom);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to add classroom.");
        return StatusCode(500, "Internal server error");
    }
}

/// <summary>
/// Edits an existing classroom in the database.
/// </summary>
/// <param name="id">The ID of the classroom to edit.</param>
/// <param name="classroom">The updated classroom object.</param>
/// <returns>Returns the edited classroom.</returns>
[HttpPut("EditClassroom/{id}")]
[Authorize(Roles = "Admin")]
[ProducesResponseType(typeof(Classroom), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(404)]
[ProducesResponseType(500)]
public async Task<ActionResult<Classroom>> EditClassroom(int id, Classroom classroom)
{
    try
    {
        var existingClassroom = await _context.Classroom.FindAsync(id);
        if (existingClassroom == null)
        {
            return NotFound();
        }

        existingClassroom.Name = classroom.Name;
        existingClassroom.Capacity = classroom.Capacity;
        await _context.SaveChangesAsync();

        return Ok(existingClassroom);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to edit classroom with ID: {id}");
        return StatusCode(500, "Internal server error");
    }
}

    }
}
