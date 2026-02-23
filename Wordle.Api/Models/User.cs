using Microsoft.AspNetCore.Identity;

namespace Wordle.Api.Models;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}
