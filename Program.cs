using Pomodoro.Api.Data;
using Pomodoro.Api.Configurations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Carrega variáveis de ambiente primeiro
builder.Configuration.AddEnvironmentVariables();

// ✅ Injeta configurações do Spotify (vinculando à section "Spotify")
builder.Services.Configure<SpotifySettings>(builder.Configuration.GetSection("Spotify"));

// ✅ Logs de debug - pode tirar depois de testar!
Console.WriteLine($"🎯 Spotify__ClientId: {builder.Configuration["Spotify__ClientId"]}");
Console.WriteLine($"🎯 Spotify__ClientSecret: {builder.Configuration["Spotify__ClientSecret"]}");
Console.WriteLine($"🎯 Spotify__RedirectUri: {builder.Configuration["Spotify__RedirectUri"]}");

// ✅ Define a porta dinamicamente para Railway ou local
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// ✅ Injeta dependências
builder.Services.AddHttpClient(); // necessário pro SpotifyAuthController
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Conexão com o PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Política CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "https://pomodoro-focus-ten.vercel.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// ✅ Builda a aplicação
var app = builder.Build();

// ✅ Aplica migrações automáticas (cuidado: em produção pode querer desativar!)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// ✅ Configuração do Swagger em dev e prod
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Middlewares principais
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization(); // habilitar se houver autenticação
app.MapControllers();

// ✅ Roda o app
app.Run();
