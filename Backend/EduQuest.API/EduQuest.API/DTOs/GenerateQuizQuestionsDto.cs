using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    // ------------------------------------------------------------------
    // Request body for POST /api/quizzes/{quizId}/questions/generate
    // ------------------------------------------------------------------
    public class GenerateQuizQuestionsDto
    {
        // How many questions to ask Gemini for. Capped at 10 per request so a
        // single call can't blow up the AI response size or take too long.
        [Range(1, 10)]
        public int Count { get; set; } = 5;

        // Optional override — if not supplied we fall back to the Quiz's own
        // Difficulty field (set when the quiz itself was created).
        [StringLength(20)]
        public string? Difficulty { get; set; }

        // Optional: only use these specific StudyMaterial records as the source
        // material (they must belong to this quiz's Topic). If left null/empty,
        // we automatically use every study material uploaded for the quiz's Topic.
        public List<int>? StudyMaterialIds { get; set; }
    }

    // ------------------------------------------------------------------
    // A single AI-generated question, including its 4 options and which
    // one is correct. This is what the admin sees when reviewing/approving,
    // and later what a learner sees when reviewing a completed attempt.
    // ------------------------------------------------------------------
    public class GeneratedQuizQuestionDto
    {
        public int QuizQuestionID { get; set; }
        public required string QuestionText { get; set; }

        // Why the correct option is correct — shown to learners when they
        // review their finished quiz attempt.
        public required string Explanation { get; set; }

        // "Yes"/"No" flags (matches the existing QuizQuestion entity design).
        public required string GeneratedByAI { get; set; }
        public required string ApprovedByAdmin { get; set; }

        public int QuizID { get; set; }

        // The 4 multiple-choice options. Each QuizOptionDto carries IsCorrect,
        // so the correct answer is stored and available for review — it's just
        // up to the learner-facing screens to hide IsCorrect until after submission.
        public List<QuizOptionDto> Options { get; set; } = new();
    }

    // ------------------------------------------------------------------
    // Wrapper returned by the generate endpoint: the saved questions plus
    // human-readable notes (e.g. "2 study materials skipped: unsupported
    // file type", "no study material found, used topic name only").
    // ------------------------------------------------------------------
    public class GenerateQuizQuestionsResponseDto
    {
        public List<GeneratedQuizQuestionDto> Questions { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }
}
