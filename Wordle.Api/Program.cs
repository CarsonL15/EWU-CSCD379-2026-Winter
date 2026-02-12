using Microsoft.EntityFrameworkCore;
using Wordle.Api.Data;
using Wordle.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework - use SQL Server if connection string exists, otherwise SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
            sqlOptions.EnableRetryOnFailure()));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=gridseeker.db"));
}

// Register services via DI
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<TestimonialService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlServer())
    {
        // Fix broken schema from SQLite-generated migration (one-time)
        // If Games table exists without identity column, drop and re-migrate
        db.Database.ExecuteSqlRaw(@"
            IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Games')
            AND NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('Games'))
            BEGIN
                DROP TABLE [Games];
                DELETE FROM [__EFMigrationsHistory];
            END
        ");
        db.Database.Migrate();
    }
    else if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.MapControllers();

app.Run();

public partial class Program { }
