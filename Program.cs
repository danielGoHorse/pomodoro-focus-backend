using Pomodoro.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração da porta (para ambientes tipo Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

// Adiciona Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Conexão com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS (libera o frontend para acessar o backend)
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

// Spotify Config (adiciona as variáveis)
builder.Configuration.AddEnvironmentVariables(); // 👈 importante para pegar do Railway

var spotifyConfig = builder.Configuration.GetSection("Spotify");
var clientId = spotifyConfig["ClientId"];
var clientSecret = spotifyConfig["ClientSecret"];
var redirectUri = spotifyConfig["RedirectUri"];

// Exemplo de log para conferir no console
Console.WriteLine($"Spotify ClientId: {clientId}");

var app = builder.Build();

// Executa migrations automáticas
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Swagger em dev e prod
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();

// Mapeia os Controllers
app.MapControllers();

// Run app
app.Run();
