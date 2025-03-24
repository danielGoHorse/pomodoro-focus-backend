using Pomodoro.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração das portas (importante para Railway ou local)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://*:{port}");

// Adiciona IHttpClientFactory
builder.Services.AddHttpClient();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do Banco
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

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.Configure<SpotifySettings>(
    builder.Configuration.GetSection("Spotify")
);
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Cria o app
Console.WriteLine("🎯 Spotify__ClientId: " + builder.Configuration["Spotify__ClientId"]);
Console.WriteLine("🎯 Spotify__RedirectUri: " + builder.Configuration["Spotify__RedirectUri"]);
Console.WriteLine("🎯 Spotify__ClientSecret: " + builder.Configuration["Spotify__ClientSecret"]);
var app = builder.Build();
// Migrations automáticas
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

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();

app.MapControllers();



app.Run();
