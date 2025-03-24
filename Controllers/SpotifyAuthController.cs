using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;


namespace Pomodoro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private readonly SpotifySettings _spotifySettings;
        public SpotifyAuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (string.IsNullOrEmpty(_spotifySettings.ClientId) ||
                string.IsNullOrEmpty(_spotifySettings.RedirectUri))
            {
                return StatusCode(500, "❌ Spotify config ausente ou inválida.");
            }

            var scopes = "user-read-playback-state user-modify-playback-state user-read-currently-playing streaming";

            var authUrl = $"https://accounts.spotify.com/authorize" +
                          $"?response_type=code" +
                          $"&client_id={_spotifySettings.ClientId}" +
                          $"&scope={Uri.EscapeDataString(scopes)}" +
                          $"&redirect_uri={Uri.EscapeDataString(_spotifySettings.RedirectUri)}";

            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("❌ Código de autorização não recebido.");
                return BadRequest("Código de autorização não recebido.");
            }

            var clientId = _configuration["Spotify__ClientId"];
            var clientSecret = _configuration["Spotify__ClientSecret"];
            var redirectUri = _configuration["Spotify__RedirectUri"];

            Console.WriteLine("🎟️ Código de autorização recebido:");
            Console.WriteLine(code);

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

            Console.WriteLine("📡 Solicitando access token do Spotify...");

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erro ao trocar código por token: {content}");
                return BadRequest("Erro ao obter o token do Spotify.");
            }

            var tokenResponse = JsonDocument.Parse(content).RootElement;

            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            var refreshToken = tokenResponse.GetProperty("refresh_token").GetString();

            Console.WriteLine("✅ Tokens recebidos do Spotify!");
            Console.WriteLine($"Access Token: {accessToken}");
            Console.WriteLine($"Refresh Token: {refreshToken}");

            // Redireciona para o Frontend com os tokens na query string
            var frontEndUrl = "https://pomodoro-focus-ten.vercel.app";
            var redirectWithTokens = $"{frontEndUrl}/?access_token={accessToken}&refresh_token={refreshToken}";

            Console.WriteLine("🔄 Redirecionando para Frontend:");
            Console.WriteLine(redirectWithTokens);

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
}
