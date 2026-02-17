using Microsoft.EntityFrameworkCore;
using RetailSyncWeb.Data;
using RetailSyncWeb.Services;
using RetailSyncWeb.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. ПІДКЛЮЧЕННЯ БАЗИ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost\\SQLEXPRESS;Database=RetailSyncDB;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// 2. СЕРВІСИ
builder.Services.AddControllers();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- ВАЖЛИВО: Реєструємо сервіс стану (Singleton) ---
builder.Services.AddSingleton<SyncStatusService>();

// --- Фоновий процесор ---
builder.Services.AddHostedService<PackageProcessor>();

var app = builder.Build();

// 3. ІНІЦІАЛІЗАЦІЯ БАЗИ (ВИПРАВЛЕНО)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // БУЛО: db.Database.EnsureCreated(); -> Це викликає конфлікт
    // СТАЛО: Застосовуємо міграції автоматично при запуску
    db.Database.Migrate();
}

// 4. НАЛАШТУВАННЯ HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();