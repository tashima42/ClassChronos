using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using HtmlAgilityPack;
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
        
        /// <summary>
        /// Imports classes and teachers from an HTML file.
        /// </summary>
        /// <param name="htmlFile">The HTML file containing the class details.</param>
        /// <param name="idDepartment">The department ID for the teachers.</param>
        /// <returns>Returns the number of classes imported successfully.</returns>
        [HttpPost("ImportClasses")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> ImportClasses([FromForm] IFormFile htmlFile, [FromForm] int idDepartment)
        {
            try
            {
                if (htmlFile == null || htmlFile.Length == 0)
                {
                    return BadRequest("Invalid HTML file.");
                }

                // Load all teachers from the database
                var existingTeachers = await _context.Teacher.ToListAsync();

                using (var stream = new MemoryStream())
                {
                    await htmlFile.CopyToAsync(stream);
                    stream.Position = 0;

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.Load(stream);

                    var classNodes = htmlDoc.DocumentNode.SelectNodes("//td[@class='t']");
                    if (classNodes == null)
                    {
                        return BadRequest("No class details found in the HTML file.");
                    }

                    var newTeachers = new List<Teacher>();
                    var newClasses = new List<Class>();

                    // Extract teachers first
                    foreach (var classNode in classNodes)
                    {
                        var periodNode = classNode.SelectSingleNode("following-sibling::td[@class='sl']");
                        var teacherNode = periodNode?.SelectSingleNode("following-sibling::td[@class='ml']");
                        var teacherName = teacherNode?.InnerText.Trim();

                        if (!string.IsNullOrEmpty(teacherName) && !existingTeachers.Any(t => t.Name.Equals(teacherName, StringComparison.OrdinalIgnoreCase)) && !newTeachers.Any(t => t.Name.Equals(teacherName, StringComparison.OrdinalIgnoreCase)))
                        {
                            var teacher = new Teacher
                            {
                                Name = teacherName,
                                DepartmentId = idDepartment
                            };
                            newTeachers.Add(teacher);
                            existingTeachers.Add(teacher); // Add to local list to prevent duplicate entries
                        }
                    }

                    // Add new teachers to the database
                    if (newTeachers.Any())
                    {
                        await _context.Teacher.AddRangeAsync(newTeachers);
                        await _context.SaveChangesAsync();
                    }

                    // Refresh the teacher list with IDs from the database
                    existingTeachers = await _context.Teacher.ToListAsync();

                    // Extract classes with valid teacher IDs
                    foreach (var classNode in classNodes)
                    {
                        var classDetail = classNode.InnerText.Trim();
                        var nameCodeSplit = classDetail.Split(" - ");
                        if (nameCodeSplit.Length < 2) continue;

                        var className = nameCodeSplit[1];
                        var classCode = nameCodeSplit[0];
                        var number = ExtractNumberFromDetail(classDetail);
                        var periodNode = classNode.SelectSingleNode("following-sibling::td[@class='sl']");
                        var period = periodNode?.InnerText.Trim();
                        var teacherNode = periodNode?.SelectSingleNode("following-sibling::td[@class='ml']");
                        var teacherName = teacherNode?.InnerText.Trim();

                        var teacher = existingTeachers.FirstOrDefault(t => t.Name.Equals(teacherName, StringComparison.OrdinalIgnoreCase));
                        if (teacher == null) continue; // If no valid teacher found, skip the class

                        // Check if class already exists
                        if (!newClasses.Any(c => c.Name == className && c.Code == classCode && c.Period == period))
                        {
                            var newClass = new Class
                            {
                                Name = className,
                                Code = classCode,
                                Period = period,
                                Number = number,
                                TeacherId = teacher.Id
                            };

                            newClasses.Add(newClass);
                        }
                    }

                    // Add new classes to the database
                    if (newClasses.Any())
                    {
                        await _context.Class.AddRangeAsync(newClasses);
                        await _context.SaveChangesAsync();
                    }

                    return Ok(newClasses.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import classes.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        /// <summary>
        /// Changes the teacher of a class.
        /// </summary>
        /// <param name="classId">The ID of the class.</param>
        /// <param name="newTeacherId">The ID of the new teacher.</param>
        /// <returns>Returns a confirmation message.</returns>
        [HttpPost("ChangeTeacher")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> ChangeTeacher(int classId, int newTeacherId)
        {
            try
            {
                // Find the class
                var classEntity = await _context.Class.FindAsync(classId);
                if (classEntity == null)
                {
                    return NotFound("Class not found.");
                }

                // Find the new teacher
                var newTeacher = await _context.Teacher.FindAsync(newTeacherId);
                if (newTeacher == null)
                {
                    return NotFound("Teacher not found.");
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
                    PeriodOld = classEntity.Period,
                    PeriodNew = classEntity.Period,
                    Description = $"Troca de Professor ID {classEntity.TeacherId} para o ID {newTeacherId} na aula ID {classId}.",
                    LoginId = int.Parse(userId),
                    TeacherId = newTeacherId,
                    ClassId = classId
                };
                _context.Log.Add(log);

                // Update the class with the new teacher ID
                classEntity.TeacherId = newTeacherId;
                _context.Class.Update(classEntity);

                // Save the changes
                await _context.SaveChangesAsync();

                return Ok("Teacher updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change the teacher.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        /// <summary>
		/// Deletes all records in the Classes and Logs tables.
		/// </summary>
		/// <returns>Returns Ok if the records are successfully deleted.</returns>
		[HttpDelete("DeleteAllRecords")]
		[Authorize(Roles = "Admin")]
		[ProducesResponseType(200)]
		[ProducesResponseType(500)]
		public async Task<ActionResult> DeleteAllRecords()
		{
			try
			{
				// Delete all records from the Classes table
				_context.Class.RemoveRange(_context.Class);
            
				// Delete all records from the Logs table
				_context.Log.RemoveRange(_context.Log);

				// Save changes to the database
				await _context.SaveChangesAsync();

				return Ok("All records in Classes and Logs have been deleted.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to delete all records in Classes and Logs.");
				return StatusCode(500, "Internal server error");
			}
		}

        private string ExtractNumberFromDetail(string detail)
        {
            var match = System.Text.RegularExpressions.Regex.Match(detail, @"\((\d+) Aulas semanais");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
