using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutritionAI.Data;
using NutritionAI.Models;
using NutritionAI.ViewModels;
using Newtonsoft.Json;

namespace NutritionAI.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnalyticsController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var meals = await _db.Meals
                .Where(m => m.UserId == user.Id && m.CreatedAt >= thirtyDaysAgo)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            if (meals.Count == 0)
            {
                ViewBag.HasData = false;
                return View();
            }

            // Group by date
            var dailyData = meals.GroupBy(m => m.CreatedAt.Date)
                .Select(g => new DailyChartData
                {
                    Date = g.Key.ToString("MMM dd"),
                    Calories = g.Sum(m => m.TotalCalories),
                    Protein = g.Sum(m => m.TotalProtein),
                    Carbs = g.Sum(m => m.TotalCarbs),
                    Fats = g.Sum(m => m.TotalFats),
                    Meals = g.Count()
                }).ToList();

            // Meal type distribution
            var mealDist = meals.GroupBy(m => m.MealType)
                .Select(g => new DistributionData
                {
                    Name = char.ToUpper(g.Key[0]) + g.Key[1..],
                    Value = g.Count()
                }).ToList();

            // Macro distribution
            var totalProtein = meals.Sum(m => m.TotalProtein);
            var totalCarbs = meals.Sum(m => m.TotalCarbs);
            var totalFats = meals.Sum(m => m.TotalFats);
            var totalMacros = totalProtein * 4 + totalCarbs * 4 + totalFats * 9;

            var macroDist = totalMacros > 0 ? new List<MacroDistributionData>
            {
                new() { Name = "Protein", Value = (int)Math.Round((double)(totalProtein * 4) / totalMacros * 100), Color = "#10b981" },
                new() { Name = "Carbs", Value = (int)Math.Round((double)(totalCarbs * 4) / totalMacros * 100), Color = "#06b6d4" },
                new() { Name = "Fats", Value = (int)Math.Round((double)(totalFats * 9) / totalMacros * 100), Color = "#14b8a6" },
            } : new List<MacroDistributionData>();

            var vm = new AnalyticsViewModel
            {
                ChartData = dailyData,
                MealDistribution = mealDist,
                MacroDistribution = macroDist,
                TotalMeals = meals.Count,
                AvgCalories = meals.Count > 0 ? (int)Math.Round((double)meals.Sum(m => m.TotalCalories) / meals.Count) : 0,
                TotalProtein = totalProtein,
                TotalCarbs = totalCarbs,
                TotalFats = totalFats
            };

            ViewBag.HasData = true;
            ViewBag.ChartDataJson = JsonConvert.SerializeObject(dailyData);
            ViewBag.MealDistJson = JsonConvert.SerializeObject(mealDist);
            ViewBag.MacroDistJson = JsonConvert.SerializeObject(macroDist);

            return View(vm);
        }
    }
}
