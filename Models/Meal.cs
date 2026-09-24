using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutritionAI.Models
{
    [Table("meals")]
    public class Meal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? UserEmail { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        public string MealType { get; set; } = "meal";

        public int TotalCalories { get; set; }
        public int TotalProtein { get; set; }
        public int TotalCarbs { get; set; }
        public int TotalFats { get; set; }
        public string? HealthTip { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public List<Food> Foods { get; set; } = new();

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
