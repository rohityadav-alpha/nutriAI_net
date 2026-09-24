using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutritionAI.Data;
using NutritionAI.Models;
using NutritionAI.ViewModels;

namespace NutritionAI.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(AppDbContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id,
                    UserEmail = user.Email ?? "",
                    Name = user.FullName
                };
                _db.UserProfiles.Add(profile);
                await _db.SaveChangesAsync();
            }

            var vm = new ProfileViewModel
            {
                Name = profile.Name ?? user.FullName,
                Email = user.Email,
                Weight = profile.Weight,
                Height = profile.Height,
                Age = profile.Age,
                Gender = profile.Gender,
                Goal = profile.Goal,
                ActivityLevel = profile.ActivityLevel,
                TargetCalories = profile.TargetCalories,
                TargetProtein = profile.TargetProtein,
                TargetCarbs = profile.TargetCarbs,
                TargetFats = profile.TargetFats
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { error = "Not authenticated" });

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id,
                    UserEmail = user.Email ?? ""
                };
                _db.UserProfiles.Add(profile);
            }

            profile.Name = model.Name;
            profile.Weight = model.Weight;
            profile.Height = model.Height;
            profile.Age = model.Age;
            profile.Gender = model.Gender;
            profile.Goal = model.Goal;
            profile.ActivityLevel = model.ActivityLevel;
            profile.UpdatedAt = DateTime.UtcNow;

            // Calculate targets
            if (model.Weight != null && model.Height != null && model.Age != null
                && !string.IsNullOrEmpty(model.Gender) && !string.IsNullOrEmpty(model.ActivityLevel)
                && !string.IsNullOrEmpty(model.Goal))
            {
                var targets = CalculateTargets(model);
                profile.TargetCalories = targets.TargetCalories;
                profile.TargetProtein = targets.TargetProtein;
                profile.TargetCarbs = targets.TargetCarbs;
                profile.TargetFats = targets.TargetFats;
            }

            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                targetCalories = profile.TargetCalories,
                targetProtein = profile.TargetProtein,
                targetCarbs = profile.TargetCarbs,
                targetFats = profile.TargetFats
            });
        }

        private (int TargetCalories, int TargetProtein, int TargetCarbs, int TargetFats) CalculateTargets(ProfileViewModel data)
        {
            double w = data.Weight!.Value;
            double h = data.Height!.Value;
            int a = data.Age!.Value;

            double bmr = data.Gender == "male"
                ? 10 * w + 6.25 * h - 5 * a + 5
                : 10 * w + 6.25 * h - 5 * a - 161;

            var multipliers = new Dictionary<string, double>
            {
                { "sedentary", 1.2 }, { "light", 1.375 }, { "moderate", 1.55 },
                { "active", 1.725 }, { "veryActive", 1.9 }
            };

            double tdee = bmr * multipliers.GetValueOrDefault(data.ActivityLevel ?? "moderate", 1.55);

            int targetCalories = data.Goal switch
            {
                "lose" => (int)Math.Round(tdee - 500),
                "gain" => (int)Math.Round(tdee + 300),
                _ => (int)Math.Round(tdee)
            };

            int targetProtein = Math.Max((int)Math.Round(w * 2), 50);
            int targetFats = Math.Max((int)Math.Round(w * 0.9), 30);
            int carbsCal = targetCalories - (targetProtein * 4) - (targetFats * 9);
            int targetCarbs = Math.Max((int)Math.Round((double)carbsCal / 4), 100);

            return (targetCalories, targetProtein, targetCarbs, targetFats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
