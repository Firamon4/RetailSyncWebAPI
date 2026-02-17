using Microsoft.AspNetCore.Mvc;
using RetailSyncWeb.Data;
using RetailSyncWeb.Entities;
using RetailSyncWeb.Models;

namespace RetailSyncWeb.Controllers
{
    [Route("api/sync")]
    [ApiController]
    public class IngestionController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public IngestionController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // POST http://localhost:5000/api/sync/push
        [HttpPost("push")]
        public async Task<IActionResult> Push([FromBody] SyncPackage package)
        {
            // 1. ПЕРЕВІРКА БЕЗПЕКИ (API Key)
            // Очікуємо заголовок "X-API-KEY" зі значенням з appsettings.json
            var apiKey = _config["SyncSettings:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                if (!Request.Headers.TryGetValue("X-API-KEY", out var extractedApiKey) || extractedApiKey != apiKey)
                {
                    return Unauthorized(new { error = "Invalid API Key" });
                }
            }

            if (package == null) return BadRequest();

            // 2. Встановлюємо початкові значення
            package.CreatedAtUtc = DateTime.UtcNow;
            package.Status = SyncStatus.New; // Явно ставимо статус
            package.ErrorMessage = null;
            package.ProcessedAtUtc = null;

            // 3. Зберігаємо
            _db.SyncPackages.Add(package);
            await _db.SaveChangesAsync();

            return Ok(new { status = "Queued", id = package.Id });
        }
    }
}