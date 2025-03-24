using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pomodoro.Api.Configurations; // ⬅️ a pasta que criamos
using System.Text;
using System.Text.Json;

namespace Pomodoro.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SpotifySettings _spotifySettings;

        public SpotifyAuthController(
            IHttpClientFactory httpClientFactory,
            IOptions<SpotifySettings> spotifyOptions
        )
        {
            _httpClientFactory = httpClientFactory;
            _spotifySettings = spotifyOptions.Value; // 🎯 Aqui é onde ele carrega
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            Console.WriteLine($"🎯 ClientId: {_spotifySettings.ClientId}");
            Console.WriteLine($"🎯 RedirectUri: {_spotifySettings.RedirectUri}");
            Console.WriteLine($"🎯 ClientSecret: {_spotifySettings.ClientSecret}");

            if (string.IsNullOrEmpty(_spotifySettings.ClientId) || string.IsNullOrEmpty(_spotifySettings.RedirectUri))
            {
                return StatusCode(500, "❌ Variáveis de ambiente do Spotify não estão configuradas corretamente.");
            }

            var scopes = "user-read-playback-state user-modify-playback-state user-read-currently-playing streaming";

            var authUrl = $"https://accounts.spotify.com/authorize" +
                          $"?response_type=code" +
                          $"&client_id={_spotifySettings.ClientId}" +
                          $"&scope={Uri.EscapeDataString(scopes)}" +
                          $"&redirect_uri={Uri.EscapeDataString(_spotifySettings.RedirectUri)}";

            Console.WriteLine($"🔗 Redirecionando para: {authUrl}");

            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            Console.WriteLine($"🎟️ Código de autorização recebido: {code}");
            Console.WriteLine($"🎯 ClientId: {_spotifySettings.ClientId}");
            Console.WriteLine($"🎯 RedirectUri: {_spotifySettings.RedirectUri}");
            Console.WriteLine($"🎯 ClientSecret: {_spotifySettings.ClientSecret}");

            if (string.IsNullOrEmpty(_spotifySettings.ClientId) || string.IsNullOrEmpty(_spotifySettings.ClientSecret))
            {
                return StatusCode(500, "❌ Variáveis do Spotify ausentes.");
            }

            var client = _httpClientFactory.CreateClient();

            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_spotifySettings.ClientId}:{_spotifySettings.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Add("Authorization", $"Basic {basicAuth}");

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _spotifySettings.RedirectUri }
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

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var clientId = _spotifySettings.ClientId;
            var clientSecret = _spotifySettings.ClientSecret;

            var client = _httpClientFactory.CreateClient();

            var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            request.Headers.Add("Authorization", $"Basic {basicAuth}");

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "grant_type", "refresh_token" },
        { "refresh_token", refreshToken }
    });

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Erro ao renovar token: {content}");
                return BadRequest(content);
            }

            Console.WriteLine($"✅ Novo access token retornado!");

            return Ok(content);
        }

    }
}
