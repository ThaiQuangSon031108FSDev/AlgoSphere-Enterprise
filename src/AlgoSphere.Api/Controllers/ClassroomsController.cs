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
public class ClassroomsController : ControllerBase
{
    private readonly IAlgoSphereDbContext _context;

    public ClassroomsController(IAlgoSphereDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Classroom>>> GetMyClassrooms()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var classrooms = await _context.Classrooms
            .Include(c => c.Teacher)
            .Include(c => c.Organization)
            .Where(c => c.TeacherId == userId || c.Students.Any(s => s.Id == userId))
            .ToListAsync();

        return Ok(classrooms);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<Classroom>> Create(CreateClassroomDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var classroom = new Classroom
        {
            Name = dto.Name,
            OrganizationId = dto.OrganizationId,
            TeacherId = userId,
            JoinCode = Guid.NewGuid().ToString("N")[..6].ToUpper(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Classrooms.Add(classroom);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyClassrooms), new { id = classroom.Id }, classroom);
    }

    [HttpPost("join")]
    public async Task<ActionResult> Join(JoinClassroomDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var classroom = await _context.Classrooms
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.JoinCode == dto.JoinCode);

        if (classroom == null)
            return NotFound("Mã lớp học không hợp lệ.");

        if (classroom.Students.Any(s => s.Id == userId))
            return BadRequest("Bạn đã tham gia lớp học này rồi.");

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            classroom.Students.Add(user);
            await _context.SaveChangesAsync();
        }

        return Ok(new { Message = "Tham gia lớp học thành công!", ClassroomName = classroom.Name });
    }
}

public record CreateClassroomDto(string Name, int OrganizationId);
public record JoinClassroomDto(string JoinCode);
