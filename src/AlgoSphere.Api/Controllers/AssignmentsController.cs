using AlgoSphere.Application.Interfaces;
using AlgoSphere.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AlgoSphere.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAlgoSphereDbContext _context;

    public AssignmentsController(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    [HttpGet("classroom/{classroomId}")]
    public async Task<ActionResult<IEnumerable<Assignment>>> GetClassroomAssignments(int classroomId)
    {
        var assignments = await _context.Assignments
            .Include(a => a.Exercise)
            .Where(a => a.ClassroomId == classroomId)
            .OrderBy(a => a.Deadline)
            .ToListAsync();

        return Ok(assignments);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<Assignment>> Create(CreateAssignmentDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Verify classroom belongs to teacher
        var classroom = await _context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == dto.ClassroomId && c.TeacherId == userId);
        
        if (classroom == null)
            return Forbid();

        var assignment = new Assignment
        {
            Title = dto.Title,
            ClassroomId = dto.ClassroomId,
            ExerciseId = dto.ExerciseId,
            Deadline = dto.Deadline,
            CreatedAt = DateTime.UtcNow
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClassroomAssignments), new { classroomId = assignment.ClassroomId }, assignment);
    }
}

public record CreateAssignmentDto(string Title, int ClassroomId, int ExerciseId, DateTime Deadline);
