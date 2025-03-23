using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
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
        var clientId = _configuration["Spotify:ClientId"];
        var redirectUri = _configuration["Spotify:RedirectUri"];
        var scopes = "user-read-playback-state user-modify-playback-state playlist-read-private";

        var queryParams = new Dictionary<string, string>
        {
            {"client_id", clientId},
            {"response_type", "code"},
            {"redirect_uri", redirectUri},
            {"scope", scopes}
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        var authUrl = $"https://accounts.spotify.com/authorize?{queryString}";

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code)
    {
        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];
        var redirectUri = _configuration["Spotify:RedirectUri"];

        var tokenEndpoint = "https://accounts.spotify.com/api/token";

        var requestBody = new Dictionary<string, string>
        {
            {"grant_type", "authorization_code"},
            {"code", code},
            {"redirect_uri", redirectUri}
        };

        var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestBody)
        };

        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        request.Headers.Add("Authorization", $"Basic {authHeader}");

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BadRequest(responseContent);

        var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);

        // Aqui você pode salvar os tokens no banco de dados para o usuário autenticado

        // Agora, gere seu JWT e devolva ao frontend
        var jwt = GenerateJwtToken("userId"); // Troque pelo user real

        return Ok(new { token = jwt });
    }

    private string GenerateJwtToken(string userId)
    {
        // Código básico pra gerar JWT (pode ser adaptado ao seu esquema de auth)
        // Use JwtSecurityTokenHandler, ClaimsIdentity etc.
        return "your-jwt-here";
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
