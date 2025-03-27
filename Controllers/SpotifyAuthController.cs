using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Text.Json.Serialization;

namespace PomodoroFocus.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotifyAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SpotifyAuthController> _logger;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public SpotifyAuthController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<SpotifyAuthController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _clientId = _configuration["Spotify:ClientId"];
            _clientSecret = _configuration["Spotify:ClientSecret"];
            _redirectUri = _configuration["Spotify:RedirectUri"];
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            // Junte tudo que usa no front
            var scopes = new List<string>
    {
        "user-read-private",
        "user-read-email",
        "playlist-read-private",
        "playlist-read-collaborative",
        "user-modify-playback-state",
        "user-read-playback-state",
        "user-read-currently-playing",
        "streaming"
    };

            var authorizeUrl = "https://accounts.spotify.com/authorize"
                + "?response_type=code"
                + $"&client_id={_clientId}"
                + $"&scope={HttpUtility.UrlEncode(string.Join(" ", scopes))}"
                + $"&redirect_uri={HttpUtility.UrlEncode(_redirectUri)}";

            return Redirect(authorizeUrl);
        }

        /// <summary>
        /// Callback chamado pelo Spotify com o código de autorização
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Código não informado!");

            _logger.LogInformation("🎟️ Código de autorização recebido: {Code}", code);

            var tokens = await GetTokensAsync(code);

            if (tokens == null)
                return BadRequest("Erro ao obter tokens do Spotify.");

            // Frontend URL para onde você quer mandar os tokens!
            //LOCAL
            // var frontendUrl = "http://localhost:3000"; 
            //PRODUÇÃO
            var frontendUrl = "https://pomodoro-focus-ten.vercel.app/";

            // var redirectUrl = $"{frontendUrl}/?access_token={tokens.AccessToken}&refresh_token={tokens.RefreshToken}";

            _logger.LogInformation("✅ Tokens recebidos do Spotify");
            _logger.LogInformation("Access Token: {AccessToken}", tokens.AccessToken);
            _logger.LogInformation("Refresh Token: {RefreshToken}", tokens.RefreshToken);

            var redirectUrl = $"{frontendUrl}/?access_token={tokens.AccessToken}&refresh_token={tokens.RefreshToken}&expires_in={tokens.ExpiresIn}";

            _logger.LogInformation("🔗 Redirecionando para: {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Endpoint para renovar o Access Token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                _logger.LogWarning("🚫 Refresh token não enviado");
                return BadRequest("Refresh token não informado!");
            }

            _logger.LogInformation("🔄 Solicitando novo Access Token com Refresh Token...");

            var token = await RefreshAccessTokenAsync(request.RefreshToken);

            if (token == null)
            {
                _logger.LogError("❌ Falha ao renovar access token");
                return BadRequest("Erro ao renovar access token.");
            }

            return Ok(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn
            });
        }

        #region Private Methods

        private async Task<SpotifyTokenResponse?> GetTokensAsync(string code)
        {
            using var client = _httpClientFactory.CreateClient();

            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var postData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _redirectUri }
            };

            var content = new FormUrlEncodedContent(postData);

            var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ Erro ao obter tokens do Spotify: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📡 Resposta do Spotify: {Json}", json);

            return JsonSerializer.Deserialize<SpotifyTokenResponse>(json);
        }

        private async Task<SpotifyTokenResponse?> RefreshAccessTokenAsync(string refreshToken)
        {
            using var client = _httpClientFactory.CreateClient();

            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var postData = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var content = new FormUrlEncodedContent(postData);

            var response = await client.PostAsync("https://accounts.spotify.com/api/token", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("❌ Erro ao renovar access token do Spotify: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("✅ Novo Access Token recebido: {Json}", json);

            return JsonSerializer.Deserialize<SpotifyTokenResponse>(json);
        }

        #endregion
    }

    #region DTOspublic class SpotifyTokenResponse
    public class SpotifyTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
    #endregion
}
