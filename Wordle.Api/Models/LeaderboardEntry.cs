namespace Wordle.Api.Models;

public class LeaderboardEntry
{
    public string PlayerName { get; set; } = string.Empty;
    public int HighScore { get; set; }
    public int GamesPlayed { get; set; }
    public double AverageScore { get; set; }
    public int TotalTreasuresFound { get; set; }
}
