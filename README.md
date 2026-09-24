# NutriAI - AI-Powered Nutrition Tracker

A full-stack nutrition tracking web application built with **ASP.NET Core MVC**, **MySQL**, and **Google Gemini AI**. Simply snap a photo of your meal and get instant calorie and macro analysis powered by artificial intelligence.

---

## Features

- **AI Food Scanner** - Upload a photo of any meal and get detailed nutritional breakdown (calories, protein, carbs, fats) using Google Gemini AI vision capabilities
- **Dashboard** - Real-time overview of daily intake, weekly calorie trends, and recent meal analysis with interactive charts
- **Meal History** - Complete log of all scanned meals with filtering by meal type and date range
- **Analytics** - 30-day nutrition insights with line charts, bar charts, pie charts, and doughnut charts powered by Chart.js
- **Nutrition Calculator** - BMR and TDEE calculator using the Mifflin-St Jeor equation with personalized macro recommendations
- **Profile Management** - Set personal details, fitness goals, and activity level to get auto-calculated daily nutrition targets
- **Authentication** - Secure user registration and login with ASP.NET Core Identity

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core MVC (.NET 10) |
| **Language** | C# |
| **Database** | MySQL 8.0 with Entity Framework Core |
| **Authentication** | ASP.NET Core Identity (Cookie-based) |
| **AI Integration** | Google Gemini 1.5 Flash (REST API) |
| **Frontend** | HTML5, CSS3 (Vanilla), JavaScript (ES6+) |
| **Charts** | Chart.js 4.x |
| **Icons** | Lucide Icons |
| **Containerization** | Docker (multi-stage build) |

---

## Project Structure

```
NutritionAI/
├── Controllers/
│   ├── HomeController.cs           # Landing page
│   ├── AccountController.cs        # Login, Register, Logout
│   ├── DashboardController.cs      # AI Scanner, Meal saving, Daily stats
│   ├── AnalyticsController.cs      # 30-day charts and insights
│   ├── HistoryController.cs        # Meal history with filters
│   ├── CalculatorController.cs     # BMR/TDEE/Macro calculator
│   └── ProfileController.cs       # User profile and targets
├── Models/
│   ├── ApplicationUser.cs          # Extended Identity user
│   ├── UserProfile.cs              # Profile, goals, daily targets
│   ├── Meal.cs                     # Meal entry with macros
│   └── Food.cs                     # Individual food items
├── ViewModels/
│   └── ViewModels.cs               # Request/Response DTOs
├── Data/
│   └── AppDbContext.cs             # EF Core DbContext with relationships
├── Services/
│   └── GeminiService.cs           # Google Gemini AI REST client
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml          # Main layout with CDN scripts
│   │   └── _Navbar.cshtml          # Responsive navigation bar
│   ├── Home/Index.cshtml           # Landing page
│   ├── Account/
│   │   ├── Login.cshtml            # Sign in form
│   │   └── Register.cshtml         # Registration form
│   ├── Dashboard/Index.cshtml      # Dashboard with AI scanner
│   ├── Analytics/Index.cshtml      # Charts and insights
│   ├── History/Index.cshtml        # Meal history log
│   ├── Calculator/Index.cshtml     # Nutrition calculator
│   └── Profile/Index.cshtml       # Profile settings
├── wwwroot/css/
│   └── site.css                    # Complete design system
├── Program.cs                      # App configuration and middleware
├── appsettings.json                # Configuration (DB, API keys)
├── Dockerfile                      # Multi-stage Docker build
└── NutritionAI.csproj              # Project dependencies
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MySQL 8.0+](https://dev.mysql.com/downloads/)
- [Google Gemini API Key](https://aistudio.google.com/apikey)

---

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/nutrition-ai.git
cd nutrition-ai/NutritionAI
```

### 2. Set Up MySQL Database

```sql
CREATE DATABASE nutrition_ai CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 3. Configure the Application

Edit `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nutrition_ai;User=root;Password=YOUR_PASSWORD;"
  },
  "GoogleApiKey": "YOUR_GEMINI_API_KEY"
}
```

### 4. Run the Application

```bash
dotnet run
```

The app will start at `http://localhost:5000` and automatically create all database tables on first run.

---

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | Landing page |
| GET | `/Account/Login` | Login page |
| POST | `/Account/Login` | Authenticate user |
| GET | `/Account/Register` | Registration page |
| POST | `/Account/Register` | Create account |
| GET | `/Dashboard` | Dashboard with scanner |
| POST | `/Dashboard/AnalyzeFood` | Upload image for AI analysis |
| POST | `/Dashboard/SaveMeal` | Save analyzed meal to DB |
| GET | `/Analytics` | 30-day analytics charts |
| GET | `/History` | Meal history (supports query filters) |
| POST | `/Calculator/Calculate` | Calculate BMR/TDEE/Macros |
| GET | `/Profile` | Profile settings page |
| POST | `/Profile/Save` | Update profile and targets |

---

## Deployment (Railway)

### Using Docker

```bash
docker build -t nutrition-ai .
docker run -p 5000:5000 -e MYSQL_URL="mysql://user:pass@host:3306/dbname" -e GoogleApiKey="your-key" nutrition-ai
```

### Using Railway

1. Push code to GitHub
2. Create a new project on [railway.com](https://railway.com)
3. Deploy from your GitHub repo
4. Add a MySQL plugin
5. Set environment variables:
   - `MYSQL_URL` - Reference to MySQL service
   - `GoogleApiKey` - Your Gemini API key
6. Generate a domain under Settings > Networking

---

## How the AI Scanner Works

1. User uploads a meal photo from the Dashboard
2. The image is converted to base64 and sent to the `AnalyzeFood` endpoint
3. `GeminiService` sends the image to Google Gemini 1.5 Flash with a structured prompt
4. Gemini returns a JSON response with detected food items, portions, and nutritional values
5. The response is displayed to the user for review
6. User clicks "Save Meal" to store the analysis in MySQL
7. Data flows into Analytics, History, and Daily Intake calculations

---

## Database Schema

```
AspNetUsers (Identity)          UserProfiles
├── Id (PK)                     ├── Id (PK)
├── Email                       ├── UserId (FK -> AspNetUsers)
├── FullName                    ├── Weight, Height, Age, Gender
├── PasswordHash                ├── Goal, ActivityLevel
└── ...                         ├── TargetCalories/Protein/Carbs/Fats
                                └── CreatedAt, UpdatedAt

Meals                           Foods
├── Id (PK)                     ├── Id (PK)
├── UserId (FK -> AspNetUsers)  ├── MealId (FK -> Meals)
├── MealType                    ├── Name, PortionSize
├── TotalCalories               ├── Calories, Protein, Carbs, Fats
├── TotalProtein/Carbs/Fats     ├── Confidence
├── HealthTip                   └── CreatedAt
└── CreatedAt
```

---

## License

This project is open source and available under the [MIT License](LICENSE).
