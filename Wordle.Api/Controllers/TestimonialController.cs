using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wordle.Api.Models;
using Wordle.Api.Services;

namespace Wordle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestimonialController : ControllerBase
{
    private readonly TestimonialService _testimonialService;

    public TestimonialController(TestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Testimonial>>> GetTestimonials()
    {
        var testimonials = await _testimonialService.GetTestimonialsAsync();
        return Ok(testimonials);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Testimonial>> AddTestimonial([FromBody] Testimonial testimonial)
    {
        if (string.IsNullOrWhiteSpace(testimonial.Content))
        {
            return BadRequest("Content is required");
        }

        var saved = await _testimonialService.AddTestimonialAsync(testimonial);
        return Ok(saved);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTestimonial(int id)
    {
        var deleted = await _testimonialService.DeleteTestimonialAsync(id);
        if (!deleted) return NotFound();
        return Ok();
    }
}
