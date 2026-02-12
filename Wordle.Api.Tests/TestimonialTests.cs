using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wordle.Api.Data;
using Wordle.Api.Models;
using Wordle.Api.Services;

namespace Wordle.Api.Tests;

public class TestimonialServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddTestimonialAsync_PersistsToDatabase()
    {
        using var context = CreateInMemoryContext();
        var service = new TestimonialService(context);

        var testimonial = new Testimonial
        {
            Author = "TestUser",
            Content = "Great game!",
            Rating = 5
        };

        var saved = await service.AddTestimonialAsync(testimonial);
        Assert.True(saved.TestimonialId > 0);
        Assert.Equal("TestUser", saved.Author);
        Assert.Equal("Great game!", saved.Content);
        Assert.Equal(5, saved.Rating);
    }

    [Fact]
    public async Task AddTestimonialAsync_ClampsRating()
    {
        using var context = CreateInMemoryContext();
        var service = new TestimonialService(context);

        var testimonial = new Testimonial { Author = "Test", Content = "Review", Rating = 10 };
        var saved = await service.AddTestimonialAsync(testimonial);
        Assert.Equal(5, saved.Rating);
    }

    [Fact]
    public async Task GetTestimonialsAsync_ReturnsNewestFirst()
    {
        using var context = CreateInMemoryContext();
        var service = new TestimonialService(context);

        await service.AddTestimonialAsync(new Testimonial { Author = "First", Content = "Old review", Rating = 3 });
        await service.AddTestimonialAsync(new Testimonial { Author = "Second", Content = "New review", Rating = 5 });

        var results = await service.GetTestimonialsAsync();
        Assert.Equal(2, results.Count);
        Assert.Equal("Second", results[0].Author);
    }
}

public class TestimonialIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TestimonialIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetTestimonials_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/testimonial");
        response.EnsureSuccessStatusCode();

        var testimonials = await response.Content.ReadFromJsonAsync<List<Testimonial>>();
        Assert.NotNull(testimonials);
    }

    [Fact]
    public async Task PostTestimonial_ReturnsCreated()
    {
        var request = new { Author = "IntegrationTest", Content = "Works great!", Rating = 4 };
        var response = await _client.PostAsJsonAsync("/api/testimonial", request);
        response.EnsureSuccessStatusCode();

        var testimonial = await response.Content.ReadFromJsonAsync<Testimonial>();
        Assert.NotNull(testimonial);
        Assert.Equal("IntegrationTest", testimonial.Author);
    }

    [Fact]
    public async Task PostTestimonial_EmptyContent_ReturnsBadRequest()
    {
        var request = new { Author = "Test", Content = "", Rating = 5 };
        var response = await _client.PostAsJsonAsync("/api/testimonial", request);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
