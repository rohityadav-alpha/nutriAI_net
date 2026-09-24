using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutritionAI.Models
{
    [Table("foods")]
    public class Food
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MealId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string PortionSize { get; set; } = "N/A";
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fats { get; set; }
        public string Confidence { get; set; } = "Low";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("MealId")]
        public Meal? Meal { get; set; }
    }
}
