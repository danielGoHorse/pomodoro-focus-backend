using Pomodoro.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Carrega variáveis de ambiente primeiro!
builder.Configuration.AddEnvironmentVariables();

// DEBUG: Verifique se carrega as variáveis
Console.WriteLine($"🎯 Spotify__ClientId: {builder.Configuration["Spotify__ClientId"]}");
Console.WriteLine($"🎯 Spotify__ClientSecret: {builder.Configuration["Spotify__ClientSecret"]}");
Console.WriteLine($"🎯 Spotify__RedirectUri: {builder.Configuration["Spotify__RedirectUri"]}");

// Configura porta (Railway usa PORT por padrão)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Services
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "https://pomodoro-focus-ten.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// Build e Run
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();
app.Run();
