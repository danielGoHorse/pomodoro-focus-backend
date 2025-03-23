using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/pomodorosession")]
public class SpotifyController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _clientFactory;

    public SpotifyController(IConfiguration config, IHttpClientFactory clientFactory)
    {
        _config = config;
        _clientFactory = clientFactory;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var clientId = _config["Spotify:ClientId"];
        var redirectUri = _config["Spotify:RedirectUri"];
        var scopes = "user-read-private user-read-email streaming";

        var authUrl = $"https://accounts.spotify.com/authorize" +
                      $"?client_id={clientId}" +
                      $"&response_type=code" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&scope={Uri.EscapeDataString(scopes)}";

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var clientId = _config["Spotify:ClientId"];
        var clientSecret = _config["Spotify:ClientSecret"];
        var redirectUri = _config["Spotify:RedirectUri"];

        var client = _clientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

        var body = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        request.Content = new FormUrlEncodedContent(body);

        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Aqui você pode salvar os tokens em algum lugar seguro!
        return Ok(content);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var clientId = _config["Spotify:ClientId"];
        var clientSecret = _config["Spotify:ClientSecret"];

        var client = _clientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

        var body = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        };

        request.Content = new FormUrlEncodedContent(body);

        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        return Ok(content);
    }
}
