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
    public async Task<ActionResult<NewGameResponse>> NewGame()
    {
        var response = await _gameService.GenerateNewGameAsync();
        return Ok(response);
    }

    [HttpPost("save")]
    public async Task<ActionResult<Game>> SaveGame([FromBody] SaveGameRequest request)
    {
        var saved = await _gameService.SaveGameFromRequestAsync(request);
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
