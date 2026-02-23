using Microsoft.EntityFrameworkCore;
using Wordle.Api.Data;
using Wordle.Api.Models;

namespace Wordle.Api.Services;

public class TestimonialService
{
    private readonly AppDbContext _db;

    public TestimonialService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Testimonial>> GetTestimonialsAsync()
    {
        return await _db.Testimonials
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Testimonial> AddTestimonialAsync(Testimonial testimonial)
    {
        testimonial.Author = string.IsNullOrWhiteSpace(testimonial.Author) ? "Anonymous" : testimonial.Author.Trim();
        testimonial.Content = testimonial.Content.Trim();
        testimonial.Rating = Math.Clamp(testimonial.Rating, 1, 5);
        testimonial.CreatedAt = DateTime.UtcNow;

        _db.Testimonials.Add(testimonial);
        await _db.SaveChangesAsync();
        return testimonial;
    }
}
