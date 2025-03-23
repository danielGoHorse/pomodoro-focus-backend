using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class SpotifyAuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SpotifyAuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var clientId = _configuration["Spotify__ClientId"];
        var redirectUri = _configuration["Spotify__RedirectUri"];
        var scopes = "user-read-playback-state user-modify-playback-state user-read-currently-playing streaming";

        var authUrl = $"https://accounts.spotify.com/authorize" +
                      $"?response_type=code" +
                      $"&client_id={clientId}" +
                      $"&scope={Uri.EscapeDataString(scopes)}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var clientId = _configuration["Spotify__ClientId"];
        var clientSecret = _configuration["Spotify__ClientSecret"];
        var redirectUri = _configuration["Spotify__RedirectUri"];

        var client = _httpClientFactory.CreateClient();

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Content = new FormUrlEncodedContent(requestBody)
        };

        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Erro ao trocar código por token: {content}");
            return BadRequest("Erro ao obter o token do Spotify.");
        }

        var tokenResponse = System.Text.Json.JsonDocument.Parse(content).RootElement;

        var accessToken = tokenResponse.GetProperty("access_token").GetString();
        var refreshToken = tokenResponse.GetProperty("refresh_token").GetString();

        Console.WriteLine("✅ Tokens recebidos do Spotify");
        Console.WriteLine($"Access Token: {accessToken}");
        Console.WriteLine($"Refresh Token: {refreshToken}");

        // Aqui você poderia criar um JWT seu se quiser, mas vamos redirecionar para o Front!
        var frontEndUrl = "https://pomodoro-focus-ten.vercel.app";
        var redirectWithTokens = $"{frontEndUrl}/?access_token={accessToken}&refresh_token={refreshToken}";

        return Redirect(redirectWithTokens);
    }
}

public class SpotifyTokenResponse
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string refresh_token { get; set; }
    public string scope { get; set; }
}
