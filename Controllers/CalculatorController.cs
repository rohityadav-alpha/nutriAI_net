using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NutritionAI.ViewModels;

namespace NutritionAI.Controllers
{
    [Authorize]
    public class CalculatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Calculate([FromBody] CalculatorViewModel model)
        {
            if (model.Weight == null || model.Height == null || model.Age == null)
            {
                return Json(new { error = "Please fill all fields" });
            }

            double w = model.Weight.Value;
            double h = model.Height.Value;
            int a = model.Age.Value;

            // BMR (Mifflin-St Jeor)
            double bmr = model.Gender == "male"
                ? 10 * w + 6.25 * h - 5 * a + 5
                : 10 * w + 6.25 * h - 5 * a - 161;

            var multipliers = new Dictionary<string, double>
            {
                { "sedentary", 1.2 },
                { "light", 1.375 },
                { "moderate", 1.55 },
                { "active", 1.725 },
                { "veryActive", 1.9 }
            };

            double tdee = bmr * (multipliers.GetValueOrDefault(model.ActivityLevel, 1.55));

            int calMin, calMax;
            switch (model.Goal)
            {
                case "lose":
                    calMin = (int)Math.Round(tdee - 500);
                    calMax = (int)Math.Round(tdee - 250);
                    break;
                case "gain":
                    calMin = (int)Math.Round(tdee + 250);
                    calMax = (int)Math.Round(tdee + 500);
                    break;
                default:
                    calMin = (int)Math.Round(tdee - 100);
                    calMax = (int)Math.Round(tdee + 100);
                    break;
            }

            int proteinMin = (int)Math.Round(w * 1.6);
            int proteinMax = (int)Math.Round(w * 2.2);
            int fatsMin = (int)Math.Round(w * 0.8);
            int fatsMax = (int)Math.Round(w * 1.0);
            double avgCal = (calMin + calMax) / 2.0;
            double avgProtein = (proteinMin + proteinMax) / 2.0;
            double avgFats = (fatsMin + fatsMax) / 2.0;
            double carbsCal = avgCal - (avgProtein * 4) - (avgFats * 9);
            int carbsAvg = (int)Math.Round(carbsCal / 4);
            int carbsMin = Math.Max((int)Math.Round(carbsAvg * 0.85), 100);
            int carbsMax = (int)Math.Round(carbsAvg * 1.15);

            return Json(new CalculatorResultViewModel
            {
                BMR = (int)Math.Round(bmr),
                TDEE = (int)Math.Round(tdee),
                CaloriesMin = calMin,
                CaloriesMax = calMax,
                ProteinMin = proteinMin,
                ProteinMax = proteinMax,
                CarbsMin = carbsMin,
                CarbsMax = carbsMax,
                FatsMin = fatsMin,
                FatsMax = fatsMax
            });
        }
    }
}
