using Microsoft.EntityFrameworkCore;
using Wordle.Api.Data;
using Wordle.Api.Models;
using Wordle.Api.Services;

namespace Wordle.Api.Tests;

public class GameServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void GenerateGrid_HasCorrectDimensions()
    {
        var grid = GameService.GenerateGrid(8, 8, 5, 4, seed: 42);
        Assert.Equal(8, grid.Rows);
        Assert.Equal(8, grid.Cols);
    }

    [Fact]
    public void GenerateGrid_HasCorrectTreasureCount()
    {
        var grid = GameService.GenerateGrid(8, 8, 5, 4, seed: 42);
        int treasures = 0;
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Cols; c++)
                if (grid.Cells[r, c] == CellType.Treasure)
                    treasures++;
        Assert.Equal(5, treasures);
    }

    [Fact]
    public void GenerateGrid_HasCorrectTrapCount()
    {
        var grid = GameService.GenerateGrid(8, 8, 5, 4, seed: 42);
        int traps = 0;
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Cols; c++)
                if (grid.Cells[r, c] == CellType.Trap)
                    traps++;
        Assert.Equal(4, traps);
    }

    [Fact]
    public void Scan_ReturnsTreasure_WhenCellIsTreasure()
    {
        var grid = GameService.GenerateGrid(8, 8, 5, 4, seed: 42);
        // Find a treasure cell
        for (int r = 0; r < grid.Rows; r++)
        {
            for (int c = 0; c < grid.Cols; c++)
            {
                if (grid.Cells[r, c] == CellType.Treasure)
                {
                    var result = GameService.Scan(grid, r, c);
                    Assert.Equal(CellType.Treasure, result.Type);
                    Assert.Equal(0, result.Distance);
                    return;
                }
            }
        }
    }

    [Fact]
    public void Scan_ReturnsTrap_WhenCellIsTrap()
    {
        var grid = GameService.GenerateGrid(8, 8, 5, 4, seed: 42);
        for (int r = 0; r < grid.Rows; r++)
        {
            for (int c = 0; c < grid.Cols; c++)
            {
                if (grid.Cells[r, c] == CellType.Trap)
                {
                    var result = GameService.Scan(grid, r, c);
                    Assert.Equal(CellType.Trap, result.Type);
                    return;
                }
            }
        }
    }

    [Fact]
    public void Scan_ReturnsDistance_WhenCellIsEmpty()
    {
        var grid = GameService.GenerateGrid(8, 8, 5, 4, seed: 42);
        for (int r = 0; r < grid.Rows; r++)
        {
            for (int c = 0; c < grid.Cols; c++)
            {
                if (grid.Cells[r, c] == CellType.Empty)
                {
                    var result = GameService.Scan(grid, r, c);
                    Assert.Equal(CellType.Empty, result.Type);
                    Assert.True(result.Distance > 0);
                    return;
                }
            }
        }
    }

    [Fact]
    public async Task SaveGameAsync_PersistsToDatabase()
    {
        using var context = CreateInMemoryContext();
        var service = new GameService(context);

        var game = new Game
        {
            PlayerName = "TestPlayer",
            TreasuresFound = 3,
            ScansRemaining = 5,
            LivesRemaining = 2,
            Score = 15,
            Won = true,
            DurationSeconds = 60
        };

        var saved = await service.SaveGameAsync(game);
        Assert.True(saved.GameId > 0);

        var fromDb = await context.Games.FindAsync(saved.GameId);
        Assert.NotNull(fromDb);
        Assert.Equal("TestPlayer", fromDb.PlayerName);
    }

    [Fact]
    public async Task GetLeaderboardAsync_ReturnsTopScores()
    {
        using var context = CreateInMemoryContext();
        var service = new GameService(context);

        context.Games.AddRange(
            new Game { PlayerName = "Alice", Score = 20, TreasuresFound = 4 },
            new Game { PlayerName = "Alice", Score = 30, TreasuresFound = 5 },
            new Game { PlayerName = "Bob", Score = 50, TreasuresFound = 5 },
            new Game { PlayerName = "Charlie", Score = 10, TreasuresFound = 2 }
        );
        await context.SaveChangesAsync();

        var leaderboard = await service.GetLeaderboardAsync();

        Assert.Equal(3, leaderboard.Count);
        Assert.Equal("Bob", leaderboard[0].PlayerName);
        Assert.Equal(50, leaderboard[0].HighScore);
        Assert.Equal("Alice", leaderboard[1].PlayerName);
        Assert.Equal(30, leaderboard[1].HighScore);
        Assert.Equal(2, leaderboard[1].GamesPlayed);
    }
}
