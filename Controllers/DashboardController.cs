using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutritionAI.Data;
using NutritionAI.Models;
using NutritionAI.Services;
using Newtonsoft.Json;

namespace NutritionAI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GeminiService _gemini;

        public DashboardController(AppDbContext db, UserManager<ApplicationUser> userManager, GeminiService gemini)
        {
            _db = db;
            _userManager = userManager;
            _gemini = gemini;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Get today's meals
            var today = DateTime.UtcNow.Date;
            var todayMeals = await _db.Meals
                .Where(m => m.UserId == user.Id && m.CreatedAt.Date == today)
                .ToListAsync();

            // Get weekly data
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var weeklyMeals = await _db.Meals
                .Where(m => m.UserId == user.Id && m.CreatedAt >= weekAgo)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // Get last meal
            var lastMeal = await _db.Meals
                .Include(m => m.Foods)
                .Where(m => m.UserId == user.Id)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            // Get profile for targets
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            ViewBag.UserName = user.FullName ?? user.Email;
            ViewBag.TodayCalories = todayMeals.Sum(m => m.TotalCalories);
            ViewBag.TodayProtein = todayMeals.Sum(m => m.TotalProtein);
            ViewBag.TodayCarbs = todayMeals.Sum(m => m.TotalCarbs);
            ViewBag.TodayFats = todayMeals.Sum(m => m.TotalFats);
            ViewBag.TodayMeals = todayMeals.Count;
            ViewBag.LastMeal = lastMeal;
            ViewBag.Profile = profile;

            // Weekly chart data
            var weeklyData = new List<object>();
            var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddDays(-i).Date;
                var dayMeals = weeklyMeals.Where(m => m.CreatedAt.Date == date);
                weeklyData.Add(new
                {
                    day = date.ToString("ddd"),
                    calories = dayMeals.Sum(m => m.TotalCalories),
                    protein = dayMeals.Sum(m => m.TotalProtein),
                    carbs = dayMeals.Sum(m => m.TotalCarbs)
                });
            }
            ViewBag.WeeklyDataJson = JsonConvert.SerializeObject(weeklyData);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeFood()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return Json(new { error = "No image uploaded" });
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var result = await _gemini.AnalyzeFoodImageAsync(bytes, file.ContentType);

            if (!result.Success)
            {
                return Json(new { error = result.Error });
            }

            return Json(new { success = true, data = result.Data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveMeal([FromBody] SaveMealRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { error = "Not authenticated" });

            try
            {
                var meal = new Meal
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    MealType = request.MealType ?? "meal",
                    TotalCalories = request.TotalCalories,
                    TotalProtein = request.TotalProtein,
                    TotalCarbs = request.TotalCarbs,
                    TotalFats = request.TotalFats,
                    HealthTip = request.HealthTip,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var food in request.Foods)
                {
                    meal.Foods.Add(new Food
                    {
                        Name = food.Name ?? "Unknown",
                        PortionSize = food.PortionSize ?? "N/A",
                        Calories = food.Calories,
                        Protein = food.Protein,
                        Carbs = food.Carbs,
                        Fats = food.Fats,
                        Confidence = food.Confidence ?? "Low",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _db.Meals.Add(meal);
                await _db.SaveChangesAsync();

                return Json(new { success = true, mealId = meal.Id });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to save: " + ex.Message });
            }
        }
    }

    public class SaveMealRequest
    {
        public string? MealType { get; set; }
        public int TotalCalories { get; set; }
        public int TotalProtein { get; set; }
        public int TotalCarbs { get; set; }
        public int TotalFats { get; set; }
        public string? HealthTip { get; set; }
        public List<SaveFoodRequest> Foods { get; set; } = new();
    }

    public class SaveFoodRequest
    {
        public string? Name { get; set; }
        public string? PortionSize { get; set; }
        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fats { get; set; }
        public string? Confidence { get; set; }
    }
}
