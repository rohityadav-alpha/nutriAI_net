using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NutritionAI.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["GoogleApiKey"] ?? "";
        }

        public async Task<GeminiResult> AnalyzeFoodImageAsync(byte[] imageBytes, string mimeType = "image/jpeg")
        {
            try
            {
                var base64Image = Convert.ToBase64String(imageBytes);

                var prompt = @"
You are a nutrition analysis AI. Analyze this food image and return ONLY a JSON object with no additional text, explanations, or markdown formatting.

STRICT RULES:
- Return ONLY the JSON object
- NO markdown code blocks
- NO explanations before or after JSON
- Start directly with {
- End directly with }

JSON STRUCTURE (copy exactly):
{
  ""foods"": [
    {
      ""name"": ""food item name"",
      ""portion_size"": ""estimated portion"",
      ""calories"": 0,
      ""protein"": 0,
      ""carbs"": 0,
      ""fats"": 0,
      ""confidence"": ""High""
    }
  ],
  ""total_calories"": 0,
  ""total_protein"": 0,
  ""total_carbs"": 0,
  ""total_fats"": 0,
  ""meal_type"": ""lunch"",
  ""health_tip"": ""brief tip""
}

If you cannot identify food, return:
{
  ""foods"": [{""name"": ""Unknown food"", ""portion_size"": ""N/A"", ""calories"": 0, ""protein"": 0, ""carbs"": 0, ""fats"": 0, ""confidence"": ""Low""}],
  ""total_calories"": 0,
  ""total_protein"": 0,
  ""total_carbs"": 0,
  ""total_fats"": 0,
  ""meal_type"": ""snack"",
  ""health_tip"": ""Please upload a clearer image""
}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = prompt },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = mimeType,
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.4,
                        topP = 0.95,
                        topK = 40
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new GeminiResult { Error = "AI service error. Please try again." };
                }

                // Parse Gemini response
                var responseJson = JObject.Parse(responseString);
                var text = responseJson["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                if (string.IsNullOrEmpty(text))
                {
                    return new GeminiResult { Error = "AI returned empty response." };
                }

                // Clean JSON
                text = text.Replace("```json", "").Replace("```", "").Trim();
                var firstBrace = text.IndexOf('{');
                var lastBrace = text.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    text = text.Substring(firstBrace, lastBrace - firstBrace + 1);
                }

                var data = JsonConvert.DeserializeObject<FoodAnalysisData>(text);
                if (data?.Foods == null || data.Foods.Count == 0)
                {
                    return new GeminiResult { Error = "Could not detect food. Please try a clearer photo." };
                }

                return new GeminiResult { Success = true, Data = data };
            }
            catch (Exception ex)
            {
                return new GeminiResult { Error = "Something went wrong: " + ex.Message };
            }
        }
    }

    // DTOs
    public class GeminiResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public FoodAnalysisData? Data { get; set; }
    }

    public class FoodAnalysisData
    {
        [JsonProperty("foods")]
        public List<FoodItemData> Foods { get; set; } = new();

        [JsonProperty("total_calories")]
        public int TotalCalories { get; set; }

        [JsonProperty("total_protein")]
        public int TotalProtein { get; set; }

        [JsonProperty("total_carbs")]
        public int TotalCarbs { get; set; }

        [JsonProperty("total_fats")]
        public int TotalFats { get; set; }

        [JsonProperty("meal_type")]
        public string MealType { get; set; } = "meal";

        [JsonProperty("health_tip")]
        public string HealthTip { get; set; } = "Enjoy your meal!";
    }

    public class FoodItemData
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "Unknown food";

        [JsonProperty("portion_size")]
        public string PortionSize { get; set; } = "N/A";

        [JsonProperty("calories")]
        public int Calories { get; set; }

        [JsonProperty("protein")]
        public int Protein { get; set; }

        [JsonProperty("carbs")]
        public int Carbs { get; set; }

        [JsonProperty("fats")]
        public int Fats { get; set; }

        [JsonProperty("confidence")]
        public string Confidence { get; set; } = "Low";
    }
}
