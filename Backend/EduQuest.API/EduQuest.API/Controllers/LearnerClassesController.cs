using System.Security.Claims;
using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    // The signed-in learner's classes: their enrolled subjects, the topics for
    // their grade, quiz progress and study materials.
    [Authorize]
    [ApiController]
    [Route("api/learner/classes")]
    public class LearnerClassesController : ControllerBase
    {
        // StartQuiz always gives a learner exactly this many questions.
        private const int QuestionsPerAttempt = 10;

        private readonly ApplicationDBContext _context;

        public LearnerClassesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/learner/classes
        [HttpGet]
        public async Task<ActionResult<LearnerClassesResponseDto>> GetMyClasses()
        {
            var learner = await GetLearnerAsync();
            if (learner == null)
                return NotFound("No learner profile exists for this account.");

            var gradeName = learner.Grade!.GradeName;
            var subjectIds = await GetEnrolledSubjectIdsAsync(learner.LearnerID);

            var subjects = await _context.Subjects
                .Where(s => subjectIds.Contains(s.SubjectID))
                .OrderBy(s => s.SubjectName)
                .Select(s => new { s.SubjectID, s.SubjectName })
                .ToListAsync();

            var topics = await _context.Topics
                .Where(t => subjectIds.Contains(t.SubjectID) && t.GradeLevel == gradeName)
                .Select(t => new { t.TopicID, t.TopicName, t.SubjectID })
                .ToListAsync();

            var topicIds = topics.Select(t => t.TopicID).ToList();
            var quizInfos = await LoadQuizInfoAsync(learner.LearnerID, topicIds);
            var materialCounts = await LoadMaterialCountsAsync(topicIds);

            var classes = subjects.Select(subject =>
            {
                var subjectTopicIds = topics
                    .Where(t => t.SubjectID == subject.SubjectID)
                    .Select(t => t.TopicID)
                    .ToHashSet();

                var counted = quizInfos
                    .Where(q => subjectTopicIds.Contains(q.TopicID) && (q.IsReady || q.IsCompleted))
                    .ToList();
                var completed = counted.Where(q => q.IsCompleted).ToList();

                return new LearnerClassDto
                {
                    SubjectID = subject.SubjectID,
                    SubjectName = subject.SubjectName,
                    TopicCount = subjectTopicIds.Count,
                    QuizzesCompleted = completed.Count,
                    QuizzesTotal = counted.Count,
                    ResourceCount = subjectTopicIds.Sum(id =>
                        materialCounts.TryGetValue(id, out var count) ? count : 0),
                    ProgressPercent = Percent(completed.Count, counted.Count),
                    AverageScorePercent = Average(completed)
                };
            }).ToList();

            // The next quiz the learner has not taken yet.
            var subjectNames = subjects.ToDictionary(s => s.SubjectID, s => s.SubjectName);
            var subjectByTopic = topics.ToDictionary(t => t.TopicID, t => t.SubjectID);
            var topicNameById = topics.ToDictionary(t => t.TopicID, t => t.TopicName);

            var nextQuiz = quizInfos
                .Where(q => q.IsReady && !q.IsCompleted)
                .OrderBy(q => subjectNames[subjectByTopic[q.TopicID]])
                .ThenBy(q => q.TopicID)
                .ThenBy(q => q.QuizTitle)
                .FirstOrDefault();

            UpNextQuizDto? upNext = null;
            if (nextQuiz != null)
            {
                var subjectId = subjectByTopic[nextQuiz.TopicID];
                upNext = new UpNextQuizDto
                {
                    QuizID = nextQuiz.QuizID,
                    QuizTitle = nextQuiz.QuizTitle,
                    SubjectID = subjectId,
                    SubjectName = subjectNames[subjectId],
                    TopicName = topicNameById[nextQuiz.TopicID]
                };
            }

            return Ok(new LearnerClassesResponseDto
            {
                GradeName = gradeName,
                Subjects = classes,
                UpNext = upNext
            });
        }

        // GET: api/learner/classes/{subjectId}
        [HttpGet("{subjectId:int}")]
        public async Task<ActionResult<LearnerClassDetailDto>> GetClass(int subjectId)
        {
            var learner = await GetLearnerAsync();
            if (learner == null)
                return NotFound("No learner profile exists for this account.");

            var subjectIds = await GetEnrolledSubjectIdsAsync(learner.LearnerID);
            if (!subjectIds.Contains(subjectId))
                return NotFound("You are not enrolled in this subject.");

            var subjectName = await _context.Subjects
                .Where(s => s.SubjectID == subjectId)
                .Select(s => s.SubjectName)
                .FirstOrDefaultAsync();
            if (subjectName == null)
                return NotFound("Subject not found.");

            var gradeName = learner.Grade!.GradeName;

            var topics = await _context.Topics
                .Where(t => t.SubjectID == subjectId && t.GradeLevel == gradeName)
                .OrderBy(t => t.TopicID)
                .Select(t => new { t.TopicID, t.TopicName })
                .ToListAsync();

            var topicIds = topics.Select(t => t.TopicID).ToList();
            var quizInfos = await LoadQuizInfoAsync(learner.LearnerID, topicIds);

            // Comparing against int? works whether StudyMaterial.TopicID is int or int?.
            var nullableTopicIds = topicIds.Select(id => (int?)id).ToList();
            var materials = await _context.StudyMaterials
                .Where(sm => nullableTopicIds.Contains(sm.TopicID))
                .OrderBy(sm => sm.StudyMaterialID)
                .Select(sm => new
                {
                    TopicID = (int?)sm.TopicID,
                    sm.StudyMaterialID,
                    sm.Title,
                    sm.Description,
                    sm.ResourceType,
                    sm.FileURL,
                    HasFile = sm.FileData != null && sm.FileData.Length > 0
                })
                .ToListAsync();

            var topicDtos = topics.Select(topic =>
            {
                var counted = quizInfos
                    .Where(q => q.TopicID == topic.TopicID && (q.IsReady || q.IsCompleted))
                    .ToList();
                var completedCount = counted.Count(q => q.IsCompleted);

                string status;
                if (counted.Count == 0) status = "No Quiz Yet";
                else if (completedCount == counted.Count) status = "Completed";
                else if (completedCount > 0) status = "In Progress";
                else status = "Not Started";

                // Prefer a quiz not taken yet; otherwise offer a retake.
                var next = counted.FirstOrDefault(q => q.IsReady && !q.IsCompleted);
                var isRetake = false;
                if (next == null)
                {
                    next = counted.FirstOrDefault(q => q.IsReady);
                    isRetake = next != null;
                }

                return new ClassTopicDto
                {
                    TopicID = topic.TopicID,
                    TopicName = topic.TopicName,
                    Status = status,
                    QuizzesCompleted = completedCount,
                    QuizzesTotal = counted.Count,
                    NextQuizID = next?.QuizID,
                    NextQuizTitle = next?.QuizTitle,
                    NextQuizIsRetake = isRetake,
                    Materials = materials
                        .Where(m => m.TopicID == topic.TopicID)
                        .Select(m => new ClassMaterialDto
                        {
                            StudyMaterialID = m.StudyMaterialID,
                            Title = m.Title,
                            Description = m.Description,
                            ResourceType = m.ResourceType,
                            FileURL = m.FileURL,
                            HasFile = m.HasFile
                        })
                        .ToList()
                };
            }).ToList();

            var allCounted = quizInfos.Where(q => q.IsReady || q.IsCompleted).ToList();
            var allCompleted = allCounted.Where(q => q.IsCompleted).ToList();

            return Ok(new LearnerClassDetailDto
            {
                SubjectID = subjectId,
                SubjectName = subjectName,
                GradeName = gradeName,
                QuizzesCompleted = allCompleted.Count,
                QuizzesTotal = allCounted.Count,
                ProgressPercent = Percent(allCompleted.Count, allCounted.Count),
                AverageScorePercent = Average(allCompleted),
                Topics = topicDtos
            });
        }

        // GET: api/learner/materials/{id}/file
        // Sends an uploaded study-material file. Only learners enrolled in that
        // subject for that grade can download it.
        [HttpGet("/api/learner/materials/{id:int}/file")]
        public async Task<IActionResult> DownloadMaterial(int id)
        {
            var learner = await GetLearnerAsync();
            if (learner == null)
                return NotFound("No learner profile exists for this account.");

            var material = await _context.StudyMaterials
                .Where(sm => sm.StudyMaterialID == id)
                .Select(sm => new { sm.GradeSubjectID, sm.FileData, sm.FileContentType })
                .FirstOrDefaultAsync();

            if (material == null || material.FileData == null || material.FileData.Length == 0)
                return NotFound("This material has no uploaded file.");

            var gradeSubject = await _context.GradeSubjects
                .Where(gs => gs.GradeSubjectID == material.GradeSubjectID)
                .Select(gs => new { gs.GradeID, gs.SubjectID })
                .FirstOrDefaultAsync();

            var subjectIds = await GetEnrolledSubjectIdsAsync(learner.LearnerID);

            if (gradeSubject == null ||
                gradeSubject.GradeID != learner.GradeID ||
                !subjectIds.Contains(gradeSubject.SubjectID))
            {
                return Forbid();
            }

            var contentType = string.IsNullOrWhiteSpace(material.FileContentType)
                ? "application/octet-stream"
                : material.FileContentType;

            return File(material.FileData, contentType);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private sealed class QuizInfo
        {
            public int QuizID { get; init; }
            public string QuizTitle { get; init; } = string.Empty;
            public int TopicID { get; init; }
            public bool IsReady { get; init; }
            public bool IsCompleted { get; init; }
            public int? LatestPercentage { get; init; }
        }

        private async Task<Learner?> GetLearnerAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return null;

            return await _context.Learners
                .Include(l => l.Grade)
                .FirstOrDefaultAsync(l => l.UserID == userId);
        }

        private Task<List<int>> GetEnrolledSubjectIdsAsync(int learnerId)
        {
            return _context.learnerSubjects
                .Where(ls => ls.LearnerID == learnerId)
                .Select(ls => ls.SubjectID)
                .ToListAsync();
        }

        // Published quizzes in the given topics, with readiness and the
        // learner's latest submitted result for each.
        private async Task<List<QuizInfo>> LoadQuizInfoAsync(int learnerId, List<int> topicIds)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.IsPublished && topicIds.Contains(q.TopicID))
                .OrderBy(q => q.QuizTitle)
                .Select(q => new { q.QuizID, q.QuizTitle, q.TopicID })
                .ToListAsync();

            var quizIds = quizzes.Select(q => q.QuizID).ToList();

            var questionCounts = await _context.QuizQuestions
                .Where(qq => quizIds.Contains(qq.QuizID) && qq.ApprovedByAdmin)
                .GroupBy(qq => qq.QuizID)
                .Select(g => new { QuizID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.QuizID, x => x.Count);

            // An attempt with saved answers has been submitted.
            var attempts = await _context.QuizAttempts
                .Where(a => a.LearnerID == learnerId
                         && quizIds.Contains(a.QuizID)
                         && a.QuizAttemptAnswers.Any())
                .Select(a => new
                {
                    a.QuizID,
                    a.Score,
                    a.DateTaken,
                    Total = a.QuizAttemptQuestions.Count()
                })
                .ToListAsync();

            var latestByQuiz = attempts
                .GroupBy(a => a.QuizID)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.DateTaken).First());

            return quizzes.Select(q =>
            {
                questionCounts.TryGetValue(q.QuizID, out var questionCount);
                latestByQuiz.TryGetValue(q.QuizID, out var latest);

                int? percentage = null;
                if (latest != null)
                {
                    percentage = latest.Total > 0
                        ? (int)Math.Round(latest.Score * 100.0 / latest.Total)
                        : 0;
                }

                return new QuizInfo
                {
                    QuizID = q.QuizID,
                    QuizTitle = q.QuizTitle,
                    TopicID = q.TopicID,
                    IsReady = questionCount >= QuestionsPerAttempt,
                    IsCompleted = latest != null,
                    LatestPercentage = percentage
                };
            }).ToList();
        }

        private async Task<Dictionary<int, int>> LoadMaterialCountsAsync(List<int> topicIds)
        {
            // Comparing against int? works whether StudyMaterial.TopicID is int or int?.
            var nullableTopicIds = topicIds.Select(id => (int?)id).ToList();

            var rows = await _context.StudyMaterials
                .Where(sm => nullableTopicIds.Contains(sm.TopicID))
                .GroupBy(sm => sm.TopicID)
                .Select(g => new { TopicID = g.Key, Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => (int)r.TopicID, r => r.Count);
        }

        private static int Percent(int part, int whole)
        {
            return whole == 0 ? 0 : (int)Math.Round(part * 100.0 / whole);
        }

        private static int? Average(List<QuizInfo> completed)
        {
            var scores = completed
                .Where(q => q.LatestPercentage.HasValue)
                .Select(q => q.LatestPercentage!.Value)
                .ToList();

            return scores.Count == 0 ? null : (int)Math.Round(scores.Average());
        }
    }
}