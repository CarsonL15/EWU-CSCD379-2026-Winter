using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
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

    [Fact]
    public async Task DeleteTestimonialAsync_ReturnsTrue_WhenExists()
    {
        using var context = CreateInMemoryContext();
        var service = new TestimonialService(context);

        var testimonial = await service.AddTestimonialAsync(new Testimonial { Author = "Test", Content = "To delete", Rating = 3 });
        var result = await service.DeleteTestimonialAsync(testimonial.TestimonialId);

        Assert.True(result);
        var remaining = await service.GetTestimonialsAsync();
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task DeleteTestimonialAsync_ReturnsFalse_WhenNotFound()
    {
        using var context = CreateInMemoryContext();
        var service = new TestimonialService(context);

        var result = await service.DeleteTestimonialAsync(999);
        Assert.False(result);
    }
}

public class TestimonialIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TestimonialIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));

                services.AddAuthentication("TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        });
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        return client;
    }

    [Fact]
    public async Task GetTestimonials_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/testimonial");
        response.EnsureSuccessStatusCode();

        var testimonials = await response.Content.ReadFromJsonAsync<List<Testimonial>>();
        Assert.NotNull(testimonials);
    }

    [Fact]
    public async Task PostTestimonial_WithAuth_ReturnsOk()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync("/api/testimonial",
            new { Author = "IntegrationTest", Content = "Works great!", Rating = 4 });
        response.EnsureSuccessStatusCode();

        var testimonial = await response.Content.ReadFromJsonAsync<Testimonial>();
        Assert.NotNull(testimonial);
        Assert.Equal("IntegrationTest", testimonial.Author);
    }

    [Fact]
    public async Task PostTestimonial_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/testimonial",
            new { Author = "Test", Content = "Unauthorized", Rating = 5 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostTestimonial_EmptyContent_ReturnsBadRequest()
    {
        var client = CreateAuthenticatedClient();
        var response = await client.PostAsJsonAsync("/api/testimonial",
            new { Author = "Test", Content = "", Rating = 5 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
