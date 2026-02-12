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
        // Drop all tables so EnsureCreated rebuilds with correct SQL Server types
        // (Migrations were generated with SQLite and have wrong column types for SQL Server)
        db.Database.ExecuteSqlRaw(@"
            IF OBJECT_ID('Games', 'U') IS NOT NULL DROP TABLE [Games];
            IF OBJECT_ID('Testimonials', 'U') IS NOT NULL DROP TABLE [Testimonials];
            IF OBJECT_ID('__EFMigrationsHistory', 'U') IS NOT NULL DROP TABLE [__EFMigrationsHistory];
        ");
        db.Database.EnsureCreated();
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
