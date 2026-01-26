using Microsoft.EntityFrameworkCore;
using RetailSyncWeb.Data;
using RetailSyncWeb.Services;
using RetailSyncWeb.Components;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Server=localhost\\SQLEXPRESS;Database=RetailSyncDB;Trusted_Connection=True;TrustServerCertificate=True;";

// ВАЖЛИВО: Замість AddDbContext використовуємо AddDbContextFactory
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SyncStatusService>();
builder.Services.AddHostedService<PackageProcessor>();

var app = builder.Build();

// Ініціалізація бази через фабрику
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.EnsureCreated();
}

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
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();