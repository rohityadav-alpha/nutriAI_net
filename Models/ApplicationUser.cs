using Microsoft.AspNetCore.Identity;

namespace NutritionAI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public UserProfile? Profile { get; set; }
    }
}
