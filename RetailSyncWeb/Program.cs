using Microsoft.EntityFrameworkCore;
using RetailSyncWeb.Data;
using RetailSyncWeb.Services;
using RetailSyncWeb.Components; // Для Blazor

var builder = WebApplication.CreateBuilder(args);

// 1. ПІДКЛЮЧЕННЯ БАЗИ 
var connectionString = "Server=localhost\\SQLEXPRESS;Database=RetailSyncDB;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// 2. СЕРВІСИ
builder.Services.AddControllers(); // Для API
builder.Services.AddRazorComponents().AddInteractiveServerComponents(); // Для Blazor UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Для тестування API

// Реєструємо наш фоновий процесор
builder.Services.AddHostedService<PackageProcessor>();

var app = builder.Build();

// 3. ІНІЦІАЛІЗАЦІЯ БАЗИ (Авто-створення таблиць)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
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

// Маршрути (включаємо і UI, і API)
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();