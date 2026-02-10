using Microsoft.EntityFrameworkCore;
using Wordle.Api.Data;
using Wordle.Api.Models;

namespace Wordle.Api.Services;

public class GameService
{
    private readonly AppDbContext _db;

    public GameService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Game> SaveGameAsync(Game game)
    {
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        return game;
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int count = 10)
    {
        var games = await _db.Games.ToListAsync();

        var entries = games
            .GroupBy(g => g.PlayerName)
            .Select(group => new LeaderboardEntry
            {
                PlayerName = group.Key,
                HighScore = group.Max(g => g.Score),
                GamesPlayed = group.Count(),
                AverageScore = Math.Round(group.Average(g => (double)g.Score), 1),
                TotalTreasuresFound = group.Sum(g => g.TreasuresFound)
            })
            .OrderByDescending(e => e.HighScore)
            .Take(count)
            .ToList();

        return entries;
    }

    public static GridState GenerateGrid(int rows = 8, int cols = 8, int treasureCount = 5, int trapCount = 4, int? seed = null)
    {
        var rand = seed.HasValue ? new Random(seed.Value) : new Random();
        var cells = new CellType[rows, cols];

        // Place treasures
        int placed = 0;
        while (placed < treasureCount)
        {
            int r = rand.Next(rows);
            int c = rand.Next(cols);
            if (cells[r, c] == CellType.Empty)
            {
                cells[r, c] = CellType.Treasure;
                placed++;
            }
        }

        // Place traps
        placed = 0;
        while (placed < trapCount)
        {
            int r = rand.Next(rows);
            int c = rand.Next(cols);
            if (cells[r, c] == CellType.Empty)
            {
                cells[r, c] = CellType.Trap;
                placed++;
            }
        }

        return new GridState
        {
            Rows = rows,
            Cols = cols,
            Cells = cells,
            TreasureCount = treasureCount,
            TrapCount = trapCount
        };
    }

    public static ScanResult Scan(GridState grid, int row, int col)
    {
        var cellType = grid.Cells[row, col];

        if (cellType == CellType.Treasure)
        {
            return new ScanResult { Type = CellType.Treasure, Distance = 0 };
        }

        if (cellType == CellType.Trap)
        {
            return new ScanResult { Type = CellType.Trap, Distance = 0 };
        }

        // Calculate distance to nearest treasure
        double minDist = double.MaxValue;
        for (int r = 0; r < grid.Rows; r++)
        {
            for (int c = 0; c < grid.Cols; c++)
            {
                if (grid.Cells[r, c] == CellType.Treasure)
                {
                    double dist = Math.Sqrt(Math.Pow(row - r, 2) + Math.Pow(col - c, 2));
                    minDist = Math.Min(minDist, dist);
                }
            }
        }

        return new ScanResult
        {
            Type = CellType.Empty,
            Distance = Math.Round(minDist, 1)
        };
    }
}

public enum CellType
{
    Empty,
    Treasure,
    Trap
}

public class GridState
{
    public int Rows { get; set; }
    public int Cols { get; set; }
    public CellType[,] Cells { get; set; } = new CellType[0, 0];
    public int TreasureCount { get; set; }
    public int TrapCount { get; set; }
}

public class ScanResult
{
    public CellType Type { get; set; }
    public double Distance { get; set; }
}
