using Microsoft.EntityFrameworkCore;
using RetailSyncWeb.Data;
using RetailSyncWeb.Services;
using RetailSyncWeb.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. Ï²ÄÊËŞ×ÅÍÍß ÁÀÇÈ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost\\SQLEXPRESS;Database=RetailSyncDB;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// 2. ÑÅĞÂ²ÑÈ
builder.Services.AddControllers();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- ÂÀÆËÈÂÎ: Ğåºñòğóºìî ñåğâ³ñ ñòàíó (Singleton) ---
builder.Services.AddSingleton<SyncStatusService>();

// --- Ôîíîâèé ïğîöåñîğ ---
builder.Services.AddHostedService<PackageProcessor>();

var app = builder.Build();

// 3. ²Í²Ö²ÀË²ÇÀÖ²ß ÁÀÇÈ
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 4. ÍÀËÀØÒÓÂÀÍÍß HTTP
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