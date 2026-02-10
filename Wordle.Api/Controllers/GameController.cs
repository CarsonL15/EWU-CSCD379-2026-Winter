using Microsoft.AspNetCore.Mvc;
using Wordle.Api.Models;
using Wordle.Api.Services;

namespace Wordle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;

    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("new")]
    public ActionResult<NewGameResponse> NewGame()
    {
        var grid = GameService.GenerateGrid();
        // Serialize the grid as a flat array for the client
        var cells = new List<int>();
        for (int r = 0; r < grid.Rows; r++)
        {
            for (int c = 0; c < grid.Cols; c++)
            {
                cells.Add((int)grid.Cells[r, c]);
            }
        }

        return Ok(new NewGameResponse
        {
            Rows = grid.Rows,
            Cols = grid.Cols,
            TreasureCount = grid.TreasureCount,
            TrapCount = grid.TrapCount,
            // Send the grid encoded so the client can do local scans
            // In a production game you'd keep this server-side
            Grid = cells
        });
    }

    [HttpPost("save")]
    public async Task<ActionResult<Game>> SaveGame([FromBody] SaveGameRequest request)
    {
        var game = new Game
        {
            PlayerName = string.IsNullOrWhiteSpace(request.PlayerName) ? "Guest" : request.PlayerName,
            TreasuresFound = request.TreasuresFound,
            ScansRemaining = request.ScansRemaining,
            LivesRemaining = request.LivesRemaining,
            Score = request.Score,
            Won = request.Won,
            DurationSeconds = request.DurationSeconds,
            PlayedAt = DateTime.UtcNow
        };

        var saved = await _gameService.SaveGameAsync(game);
        return Ok(saved);
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

public class SaveGameRequest
{
    public string PlayerName { get; set; } = "Guest";
    public int TreasuresFound { get; set; }
    public int ScansRemaining { get; set; }
    public int LivesRemaining { get; set; }
    public int Score { get; set; }
    public bool Won { get; set; }
    public int DurationSeconds { get; set; }
}
