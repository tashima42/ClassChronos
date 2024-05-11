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
		[HttpGet(Name = "GetDepartments")]
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
    }
}
