namespace Wordle.Api.Models;

public class Testimonial
{
    public int TestimonialId { get; set; }
    public string Author { get; set; } = "Anonymous";
    public string Content { get; set; } = "";
    public int Rating { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
