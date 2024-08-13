using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
		/// <param name="classId">The ID of the class.</param>
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

				// Check for duplicates
				var duplicatePairs = pairs.GroupBy(x => x)
										.Where(g => g.Count() > 1)
										.Select(y => y.Key)
										.ToList();

				if (duplicatePairs.Any())
				{
					return Ok(duplicatePairs);
				}

				// Extract the authenticated user's ID from the JWT token
				var userId = User.FindFirst("UserId")?.Value;
				if (userId == null)
				{
					return Unauthorized("User ID not found in token.");
				}

				// Log the change
				var log = new Log
				{
					DateTime = DateTime.UtcNow.ToString("o"),
					PeriodOld = @class.Period,
					PeriodNew = combination,
					Description = "Troca de sala/horario",
					LoginId = int.Parse(userId),
					TeacherId = @class.TeacherId,
					ClassId = classId
				};
				_context.Log.Add(log);

				// Update period for the specified class
				@class.Period = combination;
				_context.Class.Update(@class);

				// Save the changes
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
