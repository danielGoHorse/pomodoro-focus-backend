using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


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
            var clientId = _configuration["Spotify:ClientId"];
            var redirectUri = _configuration["Spotify:RedirectUri"];
            var clientSecret = _configuration["Spotify:ClientSecret"];

            Console.WriteLine($"🎯 ClientId: {clientId}");
            Console.WriteLine($"🎯 RedirectUri: {redirectUri}");
            Console.WriteLine($"🎯 ClientSecret: {clientSecret}");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            {
                return StatusCode(500, "❌ Variáveis de ambiente do Spotify não estão configuradas corretamente.");
            }

            var scopes = "user-read-playback-state user-modify-playback-state user-read-currently-playing streaming";

            var authUrl = $"https://accounts.spotify.com/authorize" +
                          $"?response_type=code" +
                          $"&client_id={clientId}" +
                          $"&scope={Uri.EscapeDataString(scopes)}" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

            Console.WriteLine($"🔗 Redirecionando para: {authUrl}");

            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            var clientId = _configuration["Spotify__ClientId"];
            var clientSecret = _configuration["Spotify__ClientSecret"];
            var redirectUri = _configuration["Spotify__RedirectUri"];

            Console.WriteLine($"🎯 ClientId: {clientId}");
            Console.WriteLine($"🎯 RedirectUri: {redirectUri}");
            Console.WriteLine($"🎯 ClientSecret: {clientSecret}");

            var client = _httpClientFactory.CreateClient();

            // Codifica client_id:client_secret em Base64
            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Add("Authorization", $"Basic {basicAuth}");

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "grant_type", "authorization_code" },
        { "code", code },
        { "redirect_uri", redirectUri }
        // NÃO precisa do client_id e client_secret aqui!
    });

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

            Console.WriteLine("✅ Tokens recebidos do Spotify");
            Console.WriteLine($"Access Token: {accessToken}");
            Console.WriteLine($"Refresh Token: {refreshToken}");

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
}
