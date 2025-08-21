using AdPlatforms.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatforms.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlatformsController(IPlatformService platformService) : ControllerBase
{
    private readonly IPlatformService _platformService = platformService;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        // Проверяем, что файл был передан и он не пустой
        if (file == null || file.Length == 0)
        {
            // Если проверка не пройдена, возвращаем ошибку 400
            return BadRequest();
        }
        
        await _platformService.LoadPlatformsAsync(file.OpenReadStream());
        
        // Ответ 204 - успешность POST-запроса без тела ответа
        return NoContent();
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string location)
    {
        // Проверяем, что параметр location был передан.
        if (string.IsNullOrWhiteSpace(location))
        {
            return BadRequest("Location parameter is required.");
        }

        // Вызываем метод поиска из нашего сервиса.
        var platforms = _platformService.FindPlatforms(location);
        
        // Возвращаем найденные площадки и статус 200 OK. ASP.NET Core сам преобразует список в JSON.
        return Ok(platforms);
    }
}