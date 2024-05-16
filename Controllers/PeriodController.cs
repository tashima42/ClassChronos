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
    public class PeriodController : ControllerBase
    {
        private readonly ILogger<PeriodController> _logger;
        private readonly DataContext _context;

        public PeriodController(ILogger<PeriodController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        /// <summary>
		/// Changes the period for the specified classrooms.
		/// </summary>
		/// <param name="combination">The new period and classroom string ex: 2T4(P005).</param>
		/// <returns>Returns Ok if the period is successfully changed, or a list of duplicate combinations if any.</returns>
		[HttpPost("ChangePeriod/{classId}/{combination}")]
		[Authorize]
		[ProducesResponseType(200)]
		[ProducesResponseType(500)]
		public async Task<ActionResult> ChangePeriod(int classId, string combination)
		{
			try
			{
				var @class = await _context.Class.FindAsync(classId);
				
				if (@class == null)
				{
					return NotFound($"Class with ID {classId} not found.");
				}

				var pairs = combination.Split('-').Select(pair => pair.Trim()).ToList();

				// Check for duplicates ???
				var duplicatePairs = pairs.GroupBy(x => x)
										.Where(g => g.Count() > 1)
										.Select(y => y.Key)
										.ToList();

				if (duplicatePairs.Any())
				{
					return Ok(duplicatePairs);
				}

				// Update period for the specified class
				@class.Period = combination;
				_context.Class.Update(@class);
				await _context.SaveChangesAsync();

				return Ok();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Failed to change period for combination: {combination}");
				return StatusCode(500, "Internal server error");
			}
		}
    }
}
