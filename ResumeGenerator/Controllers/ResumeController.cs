using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeGenerator;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;  // Для работы с асинхронными методами, такими как FirstOrDefaultAsync


[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly ResumeGeneratorContext _context;

    public ResumeController(ResumeGeneratorContext context)
    {
        _context = context;
    }

    [HttpPost("save")]
    
    public async Task<IActionResult> SaveResume([FromBody] ResumeDto resumeDto)
    {
        var token = Request.Headers["Authorization"];  // Извлекаем токен из заголовка
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Токен не предоставлен.");
        }

        var userId = User.FindFirstValue(ClaimTypes.Name); // Или другой способ получения идентификатора пользователя

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == userId);

        if (user == null)
        {
            return Unauthorized();
        }

        var resume = new Resume
        {
            Title = $"Резюме от {resumeDto.FullName}",
            Content = $"{resumeDto.Email}\n{resumeDto.Phone}\n{resumeDto.Address}\n{resumeDto.Experience}\n{resumeDto.Education}\n{resumeDto.Skills}",
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Резюме успешно сохранено." });
    }


    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyResumes()
    {
        var userId = User.FindFirstValue(ClaimTypes.Name); // Получаем email пользователя

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Пользователь не найден.");
        }

        var user = await _context.Users
            .Include(u => u.Resumes) // Загружаем связанные резюме
            .FirstOrDefaultAsync(u => u.Email == userId);

        if (user == null)
        {
            return Unauthorized("Пользователь не найден.");
        }

        var resumes = user.Resumes
            .Select(r => new
            {
                r.Id,
                r.Title,
                ContentParts = r.Content.Split('\n'), // Разбиваем Content на части
                r.CreatedAt
            })
            .Select(r => new
            {
                r.Id,
                r.Title,
                Email = r.ContentParts.Length > 0 ? r.ContentParts[0] : "Не указано",
                Phone = r.ContentParts.Length > 1 ? r.ContentParts[1] : "Не указано",
                Address = r.ContentParts.Length > 2 ? r.ContentParts[2] : "Не указано",
                Experience = r.ContentParts.Length > 3 ? r.ContentParts[3] : "Не указано",
                Education = r.ContentParts.Length > 4 ? r.ContentParts[4] : "Не указано",
                Skills = r.ContentParts.Length > 5 ? r.ContentParts[5] : "Не указано",
                CreatedAt = r.CreatedAt
            })
            .ToList();

        return Ok(resumes);
    }

}

public class ResumeDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Experience { get; set; }
    public string Education { get; set; }
    public string Skills { get; set; }
}
