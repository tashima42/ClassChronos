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
    public class ClassroomController : ControllerBase
    {
        private readonly ILogger<ClassroomController> _logger;
        private readonly DataContext _context; // Injecting your DataContext

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
		
		
    }
}
