using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeGenerator;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
// Для работы с асинхронными методами, такими как FirstOrDefaultAsync


[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly ResumeGeneratorContext _context;
    private readonly HttpClient _httpClient;

    public ResumeController(ResumeGeneratorContext context, HttpClient httpClient)
    {
        _context = context;
        _httpClient = httpClient;
    }

    [HttpPost("improve-text")]
    public async Task<IActionResult> ImproveText([FromBody] ResumeRequest request)
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Ошибка получения токена GigaChat.");
        }

        var improvedExperience = await ImproveTextWithGigaChat(request.Experience, token);
        var improvedEducation = await ImproveTextWithGigaChat(request.Education, token);
        var improvedSkills = await ImproveTextWithGigaChat(request.Skills, token);

        return Ok(new
        {
            experience = improvedExperience,
            education = improvedEducation,
            skills = improvedSkills
        });
    }

    private async Task<string> GetTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://ngw.devices.sberbank.ru:9443/api/v2/oauth")
        {
            Headers =
            {
                { "RqUID", "aa398b41-b6d1-4bde-982b-333fd08faf15" },
                { "Authorization", "Basic OTdlMzRkYzItMTIwNS00ZGRjLTg1OTktNzEyMmYyZDA5ZGJiOmI0ZjhjYmU2LThiNjctNDNjOS04N2RiLTU4ZDNhNDI3NDc3Mw==" }
            },
            Content = new StringContent("scope=GIGACHAT_API_PERS", Encoding.UTF8, "application/x-www-form-urlencoded")
        };

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            // Предположим, что токен возвращается в формате JSON в поле "access_token"
            var token = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseData)?.access_token;
            return token;
        }

        return null; // Если запрос не успешен, возвращаем null
    }

    private async Task<string> ImproveTextWithGigaChat(string text, string token)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text; // Возвращаем оригинальный текст, если он пуст
        }

        var requestBody = new
        {
            model = "GigaChat",
            messages = new[]
            {
                new { role = "system", content = "Ты профессиональный редактор. Улучши текст пользователя, так чтобы он был более пригодным для резюме. Также можешь отдельно после текста написать предложения и замечания, как можно улучшить текст." },
                new { role = "user", content = text }
            },
            stream = false,
            update_interval = 0
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsync("https://gigachat.devices.sberbank.ru/api/v1/chat/completions", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            return text; // Если ошибка, возвращаем оригинальный текст
        }

        var responseData = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseData);
        var improvedText = jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

        return improvedText ?? text;
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

    [HttpGet("get-token")]
    public async Task<IActionResult> GetToken()
    {
        var token = await GetTokenAsync();
        if (token == null)
        {
            return Unauthorized("Не удалось получить токен.");
        }

        return Ok(new { token });
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

public class ResumeRequest
{
    public string Experience { get; set; }
    public string Education { get; set; }
    public string Skills { get; set; }
}
