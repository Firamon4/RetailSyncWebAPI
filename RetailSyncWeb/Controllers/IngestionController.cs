using Microsoft.AspNetCore.Mvc;
using RetailSyncWeb.Data;
using RetailSyncWeb.Entities;

namespace RetailSyncWeb.Controllers
{
    [Route("api/sync")]
    [ApiController]
    public class IngestionController : ControllerBase
    {
        private readonly AppDbContext _db;

        public IngestionController(AppDbContext db)
        {
            _db = db;
        }

        // 1C відправляє сюди: POST http://localhost:5000/api/sync/push
        [HttpPost("push")]
        public async Task<IActionResult> Push([FromBody] SyncPackage package)
        {
            if (package == null) return BadRequest();

            // Ставимо час отримання
            package.CreatedAtUtc = DateTime.UtcNow;

            // Просто зберігаємо в чергу
            _db.SyncPackages.Add(package);
            await _db.SaveChangesAsync();

            // Відповідаємо 1С миттєво
            return Ok(new { status = "Queued", id = package.Id });
        }
    }
}