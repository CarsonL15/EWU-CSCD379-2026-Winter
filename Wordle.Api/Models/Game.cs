namespace Wordle.Api.Models;

public class Game
{
    public int GameId { get; set; }
    public string PlayerName { get; set; } = "Guest";
    public int TreasuresFound { get; set; }
    public int ScansRemaining { get; set; }
    public int LivesRemaining { get; set; }
    public int Score { get; set; }
    public bool Won { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}
