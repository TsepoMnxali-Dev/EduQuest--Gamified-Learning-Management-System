using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class LearnerProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string GradeName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;

        public DateTime EnrolledSince { get; set; }
        public List<ProfileSubjectDto> Subjects { get; set; } = new();

        public int TotalPoints { get; set; }

        // Null until the learner has earned points.
        public int? Rank { get; set; }
        public int QuizzesCompleted { get; set; }

        public List<AchievementDto> Achievements { get; set; } = new();
    }

    public class ProfileSubjectDto
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8), StringLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class SubjectChoiceDto
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public bool Enrolled { get; set; }
    }

    public class UpdateSubjectsDto
    {
        [Required, MinLength(1)]
        public List<int> SubjectIDs { get; set; } = new();
    }
}