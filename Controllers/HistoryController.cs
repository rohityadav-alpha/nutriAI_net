using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutritionAI.Data;
using NutritionAI.Models;
using Newtonsoft.Json;

namespace NutritionAI.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HistoryController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? mealType, string? startDate, string? endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var query = _db.Meals
                .Include(m => m.Foods)
                .Where(m => m.UserId == user.Id);

            // Filters
            if (!string.IsNullOrEmpty(mealType) && mealType != "all")
            {
                query = query.Where(m => m.MealType == mealType);
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(m => m.CreatedAt >= start);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                end = end.Date.AddDays(1).AddTicks(-1);
                query = query.Where(m => m.CreatedAt <= end);
            }

            var meals = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .ToListAsync();

            // Stats
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var statsQuery = _db.Meals.Where(m => m.UserId == user.Id && m.CreatedAt >= weekAgo);
            var statsMeals = await statsQuery.ToListAsync();

            ViewBag.TotalMeals = statsMeals.Count;
            ViewBag.TotalCalories = statsMeals.Sum(m => m.TotalCalories);
            ViewBag.AvgCalories = statsMeals.Count > 0 ? statsMeals.Sum(m => m.TotalCalories) / statsMeals.Count : 0;
            ViewBag.TotalProtein = statsMeals.Sum(m => m.TotalProtein);
            ViewBag.TotalCarbs = statsMeals.Sum(m => m.TotalCarbs);

            ViewBag.FilterMealType = mealType ?? "all";
            ViewBag.FilterStartDate = startDate ?? "";
            ViewBag.FilterEndDate = endDate ?? "";

            return View(meals);
        }
    }
}
