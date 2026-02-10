using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wordle.Api.Data;
using Wordle.Api.Models;

namespace Wordle.Api.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Use InMemory database for tests
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task NewGame_ReturnsValidGrid()
    {
        var response = await _client.GetAsync("/api/game/new");
        response.EnsureSuccessStatusCode();

        var game = await response.Content.ReadFromJsonAsync<NewGameResponse>();
        Assert.NotNull(game);
        Assert.Equal(8, game.Rows);
        Assert.Equal(8, game.Cols);
        Assert.Equal(64, game.Grid.Count);
        Assert.Equal(5, game.TreasureCount);
    }

    [Fact]
    public async Task SaveGame_ReturnsCreatedGame()
    {
        var request = new
        {
            PlayerName = "IntegrationTest",
            TreasuresFound = 3,
            ScansRemaining = 7,
            LivesRemaining = 2,
            Score = 21,
            Won = true,
            DurationSeconds = 45
        };

        var response = await _client.PostAsJsonAsync("/api/game/save", request);
        response.EnsureSuccessStatusCode();

        var game = await response.Content.ReadFromJsonAsync<Game>();
        Assert.NotNull(game);
        Assert.Equal("IntegrationTest", game.PlayerName);
        Assert.Equal(21, game.Score);
    }

    [Fact]
    public async Task Leaderboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/leaderboard");
        response.EnsureSuccessStatusCode();

        var entries = await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>();
        Assert.NotNull(entries);
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
