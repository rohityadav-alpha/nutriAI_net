using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutritionAI.Models
{
    [Table("user_profiles")]
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        public string? Name { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public int? Age { get; set; }
        public string? Gender { get; set; }
        public string? Goal { get; set; }
        public string? ActivityLevel { get; set; }
        public int? TargetCalories { get; set; }
        public int? TargetProtein { get; set; }
        public int? TargetCarbs { get; set; }
        public int? TargetFats { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
