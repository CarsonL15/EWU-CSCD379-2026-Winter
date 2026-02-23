using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wordle.Api.Data;
using Wordle.Api.Models;

namespace Wordle.Api.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Name, "test@test.com"),
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        });
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        return client;
    }

    [Fact]
    public async Task NewGame_WithAuth_ReturnsValidGrid()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/game/new");
        response.EnsureSuccessStatusCode();

        var game = await response.Content.ReadFromJsonAsync<NewGameResponse>();
        Assert.NotNull(game);
        Assert.Equal(8, game.Rows);
        Assert.Equal(8, game.Cols);
        Assert.Equal(64, game.Grid.Count);
        Assert.Equal(5, game.TreasureCount);
    }

    [Fact]
    public async Task SaveGame_WithAuth_ReturnsCreatedGame()
    {
        var client = CreateAuthenticatedClient();
        var saveRequest = new
        {
            PlayerName = "IntegrationTest",
            TreasuresFound = 3,
            ScansRemaining = 7,
            LivesRemaining = 2,
            Score = 21,
            Won = true,
            DurationSeconds = 45
        };

        var response = await client.PostAsJsonAsync("/api/game/save", saveRequest);
        response.EnsureSuccessStatusCode();

        var game = await response.Content.ReadFromJsonAsync<Game>();
        Assert.NotNull(game);
        Assert.Equal("IntegrationTest", game.PlayerName);
        Assert.Equal(21, game.Score);
    }

    [Fact]
    public async Task Leaderboard_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/leaderboard");
        response.EnsureSuccessStatusCode();

        var entries = await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>();
        Assert.NotNull(entries);
    }

    [Fact]
    public async Task UserList_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/user/list");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GameSave_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/game/save", new
        {
            PlayerName = "Anon",
            TreasuresFound = 1,
            ScansRemaining = 5,
            LivesRemaining = 2,
            Score = 5,
            Won = false,
            DurationSeconds = 30
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class NewGameResponse
{
    public int Rows { get; set; }
    public int Cols { get; set; }
    public int TreasureCount { get; set; }
    public int TrapCount { get; set; }
    public List<int> Grid { get; set; } = new();
}
