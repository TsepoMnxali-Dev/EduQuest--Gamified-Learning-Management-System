using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EduQuest.API.Services
{
    // ======================================================================
    // Thrown whenever something goes wrong talking to Gemini or making
    // sense of what it sent back. The controller catches this specifically
    // so it can return a clean error to the frontend instead of a 500 crash.
    // ======================================================================
    public class QuizGenerationException : Exception
    {
        public QuizGenerationException(string message, Exception? inner = null)
            : base(message, inner) { }
    }

    // ----------------------------------------------------------------------
    // Small "plain data" classes used to pass things around. These are not
    // Entity Framework entities — they only exist to move data between the
    // controller and this service.
    // ----------------------------------------------------------------------

    // One uploaded study material we're about to hand to Gemini as context.
    public record StudyMaterialInput(
        string Title,
        byte[] FileBytes,
        string MimeType);

    // Gemini's JSON reply gets deserialized straight into these two records.
    // The [JsonPropertyName] attributes tell it which JSON field maps to
    // which C# property (Gemini returns lowerCamelCase field names).
    public record GeneratedOption(
        [property: JsonPropertyName("optionText")] string OptionText,
        [property: JsonPropertyName("isCorrect")] bool IsCorrect);

    public record GeneratedQuestion(
        [property: JsonPropertyName("questionText")] string QuestionText,
        [property: JsonPropertyName("explanation")] string Explanation,
        [property: JsonPropertyName("options")] List<GeneratedOption> Options);

    // ======================================================================
    // GeminiQuizGeneratorService
    //
    // Talks to Google's Gemini API to turn either (a) real study material
    // files, or (b) a bare topic name, into ready-to-save multiple-choice
    // quiz questions. Registered in Program.cs with AddHttpClient<T>(), so
    // ASP.NET hands us a properly-pooled HttpClient automatically.
    // ======================================================================
    public class GeminiQuizGeneratorService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        // Mime types Gemini can actually read as "document" content. Anything
        // else (e.g. a .docx or .pptx upload) gets skipped by the controller
        // before it ever reaches this service — trying to send an unsupported
        // type just wastes the API call and confuses Gemini.
        public static readonly HashSet<string> SupportedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "text/plain",
            "text/csv",
            "text/html",
            "text/markdown",
            "image/png",
            "image/jpeg",
            "image/webp"
        };

        // Gemini's inline-data upload path (i.e. sending file bytes directly
        // in the request body, which is what we do below) is limited to
        // ~20MB per request total. We stay well under that so there's room
        // for the prompt text and JSON overhead too.
        private const long MaxTotalMaterialBytes = 15L * 1024 * 1024; // 15 MB

        public GeminiQuizGeneratorService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        // ------------------------------------------------------------------
        // Main entry point. `materials` may be empty — if so we fall back to
        // asking Gemini to generate questions from its general knowledge of
        // the subject/topic (still useful, just less tailored to what's
        // actually been uploaded).
        // ------------------------------------------------------------------
        public async Task<List<GeneratedQuestion>> GenerateQuestionsAsync(
            string subjectName,
            string topicName,
            string gradeLevel,
            string difficulty,
            int count,
            List<StudyMaterialInput> materials)
        {
            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new QuizGenerationException(
                    "Gemini API key is not configured. Set Gemini:ApiKey via dotnet user-secrets.");
            }

            var model = _config["Gemini:Model"] ?? "gemini-2.5-flash";

            // Build the "parts" array that goes into the request. The first
            // part is always our instructions as plain text; if we have study
            // material, each file gets appended as its own inline_data part
            // right after — Gemini reads all parts together as one prompt.
            var parts = new List<object> { new { text = BuildPrompt(subjectName, topicName, gradeLevel, difficulty, count, materials.Count > 0) } };

            foreach (var material in materials)
            {
                parts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = material.MimeType,
                        // Gemini expects file bytes as a base64 string.
                        data = Convert.ToBase64String(material.FileBytes)
                    }
                });
            }

            var requestBody = new
            {
                contents = new object[]
                {
                    new { parts = parts.ToArray() }
                },
                generationConfig = new
                {
                    // Forces Gemini to reply with JSON only (no "Here are your
                    // questions:" preamble to strip out), matching the schema
                    // below exactly.
                    responseMimeType = "application/json",
                    responseSchema = BuildResponseSchema()
                }
            };

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            using var response = await _httpClient.PostAsync(
                url,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new QuizGenerationException(
                    $"Gemini API returned {(int)response.StatusCode}: {responseBody}");
            }

            // Gemini's reply is wrapped in an envelope like:
            // { "candidates": [ { "content": { "parts": [ { "text": "<our JSON>" } ] } } ] }
            // so we have to dig into it before we get the actual question JSON.
            string? generatedJson;
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                generatedJson = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch (Exception ex)
            {
                throw new QuizGenerationException("Could not parse Gemini's response.", ex);
            }

            if (string.IsNullOrWhiteSpace(generatedJson))
            {
                throw new QuizGenerationException("Gemini returned an empty response.");
            }

            List<GeneratedQuestion>? questions;
            try
            {
                questions = JsonSerializer.Deserialize<List<GeneratedQuestion>>(
                    generatedJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                throw new QuizGenerationException("Gemini's response was not valid JSON questions.", ex);
            }

            if (questions == null || questions.Count == 0)
            {
                throw new QuizGenerationException("Gemini did not return any questions.");
            }

            // Safety net: even though we asked for exactly 4 options with one
            // correct answer via the schema, double-check before this touches
            // the database. Anything malformed just gets dropped rather than
            // saved with a missing/duplicate correct answer.
            var valid = questions
                .Where(q =>
                    !string.IsNullOrWhiteSpace(q.QuestionText) &&
                    q.Options.Count == 4 &&
                    q.Options.Count(o => o.IsCorrect) == 1)
                .ToList();

            if (valid.Count == 0)
            {
                throw new QuizGenerationException(
                    "Gemini's questions didn't match the expected format (4 options, one correct).");
            }

            return valid;
        }

        // Checks a study material's total size against our safety cap.
        // Called by the controller before adding a material to the batch.
        public static bool FitsWithinBudget(long currentTotalBytes, long newFileBytes)
            => currentTotalBytes + newFileBytes <= MaxTotalMaterialBytes;

        // ------------------------------------------------------------------
        // Builds the plain-English instructions we send Gemini alongside
        // the study material (or alone, if there's no material to attach).
        // ------------------------------------------------------------------
        private static string BuildPrompt(
            string subjectName,
            string topicName,
            string gradeLevel,
            string difficulty,
            int count,
            bool hasMaterial)
        {
            var sourceInstruction = hasMaterial
                ? "Base every question strictly on the study material attached to this request " +
                  "(the files that follow this text). Do not invent facts that aren't supported by " +
                  "the attached material."
                : "No study material was attached, so use your general knowledge of the South African " +
                  "high school curriculum for this subject and topic.";

            return
                $"You are helping build a multiple-choice quiz for a South African high school " +
                $"learner in Grade {gradeLevel}, subject \"{subjectName}\", topic \"{topicName}\". " +
                $"{sourceInstruction} " +
                $"Generate {count} original multiple-choice questions at \"{difficulty}\" difficulty. " +
                $"Each question must have exactly 4 options, with exactly one option marked as the " +
                $"correct answer (isCorrect: true) and the other three clearly wrong but plausible " +
                $"(isCorrect: false). Also include a short explanation (1-2 sentences) of why the " +
                $"correct option is right — this explanation is shown to learners afterwards when " +
                $"they review their completed quiz, so make sure the correct answer and explanation " +
                $"line up. Do not repeat questions. Keep question text under 400 characters and " +
                $"explanation text under 800 characters.";
        }

        // ------------------------------------------------------------------
        // Describes the exact JSON shape we want back, using Gemini's schema
        // format (similar idea to OpenAPI/JSON Schema). Gemini enforces this
        // shape itself, which is why we can safely deserialize straight into
        // GeneratedQuestion/GeneratedOption above without extra guesswork.
        // ------------------------------------------------------------------
        private static object BuildResponseSchema() => new
        {
            type = "ARRAY",
            items = new
            {
                type = "OBJECT",
                properties = new
                {
                    questionText = new { type = "STRING" },
                    explanation = new { type = "STRING" },
                    options = new
                    {
                        type = "ARRAY",
                        items = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                optionText = new { type = "STRING" },
                                isCorrect = new { type = "BOOLEAN" }
                            },
                            required = new[] { "optionText", "isCorrect" }
                        }
                    }
                },
                required = new[] { "questionText", "explanation", "options" }
            }
        };
    }
}
