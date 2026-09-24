using System.ComponentModel.DataAnnotations;

namespace NutritionAI.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class ProfileViewModel
    {
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
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class AnalyticsViewModel
    {
        public List<DailyChartData> ChartData { get; set; } = new();
        public List<DistributionData> MealDistribution { get; set; } = new();
        public List<MacroDistributionData> MacroDistribution { get; set; } = new();
        public int TotalMeals { get; set; }
        public int AvgCalories { get; set; }
        public int TotalProtein { get; set; }
        public int TotalCarbs { get; set; }
        public int TotalFats { get; set; }
    }

    public class DailyChartData
    {
        public string Date { get; set; } = "";
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fats { get; set; }
        public int Meals { get; set; }
    }

    public class DistributionData
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }

    public class MacroDistributionData
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
        public string Color { get; set; } = "";
    }

    public class CalculatorViewModel
    {
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; } = "male";
        public string ActivityLevel { get; set; } = "moderate";
        public string Goal { get; set; } = "maintain";
    }

    public class CalculatorResultViewModel
    {
        public int BMR { get; set; }
        public int TDEE { get; set; }
        public int CaloriesMin { get; set; }
        public int CaloriesMax { get; set; }
        public int ProteinMin { get; set; }
        public int ProteinMax { get; set; }
        public int CarbsMin { get; set; }
        public int CarbsMax { get; set; }
        public int FatsMin { get; set; }
        public int FatsMax { get; set; }
    }
}
