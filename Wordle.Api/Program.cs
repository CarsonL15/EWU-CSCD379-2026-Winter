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
        // Fix broken schema from SQLite-generated migrations (one-time)
        // Drop tables with wrong column types and re-migrate
        db.Database.ExecuteSqlRaw(@"
            -- Drop Games if it has no identity or has TEXT columns (SQLite types)
            IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Games')
            AND (
                NOT EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('Games'))
                OR EXISTS (
                    SELECT 1 FROM sys.columns c
                    JOIN sys.types t ON c.system_type_id = t.system_type_id
                    WHERE c.object_id = OBJECT_ID('Games') AND t.name = 'text'
                )
            )
            BEGIN
                DROP TABLE [Games];
            END

            -- Drop Testimonials if it has TEXT columns (SQLite types)
            IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Testimonials')
            AND EXISTS (
                SELECT 1 FROM sys.columns c
                JOIN sys.types t ON c.system_type_id = t.system_type_id
                WHERE c.object_id = OBJECT_ID('Testimonials') AND t.name = 'text'
            )
            BEGIN
                DROP TABLE [Testimonials];
            END

            -- Clear migration history so all migrations re-run cleanly
            IF EXISTS (SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory')
            BEGIN
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
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.MapControllers();

app.Run();

public partial class Program { }
