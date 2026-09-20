using EduQuest.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(
            ApplicationDBContext context,
            IConfiguration configuration)
        {
            // Find the Admin role by name
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Admin");

            if (adminRole == null)
            {
                throw new InvalidOperationException(
                    "Admin role has not been configured.");
            }

            // Get Admin credentials from User Secrets
            var adminEmail = configuration["AdminSeed:Email"];
            var adminPassword = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Admin seed credentials have not been configured.");
            }

            // Check whether an Admin already exists
            var adminExists = await context.Users
                .AnyAsync(u => u.RoleID == adminRole.RoleID);

            if (adminExists)
            {
                return;
            }

            // Create the Admin
            var admin = new User
            {
                FirstName = "EduQuest",
                LastName = "Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                IsActive = true,
                RoleID = adminRole.RoleID,
                DateCreated = DateTime.UtcNow
            };

            context.Users.Add(admin);

            await context.SaveChangesAsync();
        }

                public static async Task SeedGradesAsync(
            ApplicationDBContext context)
                {
                    var grades = new[]
                    {
                "Grade 10",
                "Grade 11",
                "Grade 12"
            };

                    foreach (var gradeName in grades)
                    {
                        var exists = await context.Grades
                            .AnyAsync(g => g.GradeName == gradeName);

                        if (!exists)
                        {
                            context.Grades.Add(new Grade
                            {
                                GradeName = gradeName
                            });
                        }
                    }

                    await context.SaveChangesAsync();
                }

                public static async Task SeedSubjectsAsync(
                    ApplicationDBContext context)
                {
                    var subjects = new[]
                    {
                "Mathematics",
                "English",
                "Physical Sciences",
                "Life Sciences",
                "Geography",
                "Life Orientation"
            };

                    foreach (var subjectName in subjects)
                    {
                        var exists = await context.Subjects
                            .AnyAsync(s => s.SubjectName == subjectName);

                        if (!exists)
                        {
                            context.Subjects.Add(new Subject
                            {
                                SubjectName = subjectName
                            });
                        }
                    }

                    await context.SaveChangesAsync();
                }
        public static async Task SeedGradeSubjectsAsync(
    ApplicationDBContext context)
        {
            var grades = await context.Grades
                .ToDictionaryAsync(g => g.GradeName, g => g.GradeID);

            var subjects = await context.Subjects
                .ToDictionaryAsync(s => s.SubjectName, s => s.SubjectID);

            var gradeNames = new[]
            {
        "Grade 10",
        "Grade 11",
        "Grade 12"
    };

            var subjectNames = new[]
            {
        "Mathematics",
        "English",
        "Physical Sciences",
        "Life Sciences",
        "Geography",
        "Life Orientation"
    };

            foreach (var gradeName in gradeNames)
            {
                foreach (var subjectName in subjectNames)
                {
                    var gradeId = grades[gradeName];
                    var subjectId = subjects[subjectName];

                    var exists = await context.GradeSubjects
                        .AnyAsync(gs =>
                            gs.GradeID == gradeId &&
                            gs.SubjectID == subjectId);

                    if (!exists)
                    {
                        context.GradeSubjects.Add(new GradeSubject
                        {
                            GradeID = gradeId,
                            SubjectID = subjectId
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedTopicsAsync(
    ApplicationDBContext context)
        {
            var subjects = await context.Subjects
                .ToDictionaryAsync(s => s.SubjectName, s => s.SubjectID);

            var topics = new[]
            {
        // =========================
        // GRADE 10 - MATHEMATICS
        // =========================
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Algebraic Expressions" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Exponents and Surds" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Equations and Inequalities" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Functions and Graphs" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Analytical Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Trigonometry" },

        // =========================
        // GRADE 10 - ENGLISH
        // =========================
        new { Subject = "English", Grade = "Grade 10", Topic = "Reading Comprehension" },
        new { Subject = "English", Grade = "Grade 10", Topic = "Summary Writing" },
        new { Subject = "English", Grade = "Grade 10", Topic = "Grammar and Language" },
        new { Subject = "English", Grade = "Grade 10", Topic = "Creative Writing" },
        new { Subject = "English", Grade = "Grade 10", Topic = "Essay Writing" },
        new { Subject = "English", Grade = "Grade 10", Topic = "Poetry" },

        // =========================
        // GRADE 10 - PHYSICAL SCIENCES
        // =========================
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Matter and Materials" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Chemical Reactions" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Mechanics" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Energy and Matter" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Waves and Sound" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Electricity" },

        // =========================
        // GRADE 10 - LIFE SCIENCES
        // =========================
        new { Subject = "Life Sciences", Grade = "Grade 10", Topic = "The Chemistry of Life" },
        new { Subject = "Life Sciences", Grade = "Grade 10", Topic = "Cells" },
        new { Subject = "Life Sciences", Grade = "Grade 10", Topic = "Cell Division" },
        new { Subject = "Life Sciences", Grade = "Grade 10", Topic = "Plant and Animal Tissues" },
        new { Subject = "Life Sciences", Grade = "Grade 10", Topic = "Biodiversity" },
        new { Subject = "Life Sciences", Grade = "Grade 10", Topic = "Human Nutrition" },

        // =========================
        // GRADE 10 - GEOGRAPHY
        // =========================
        new { Subject = "Geography", Grade = "Grade 10", Topic = "The Atmosphere" },
        new { Subject = "Geography", Grade = "Grade 10", Topic = "Geomorphology" },
        new { Subject = "Geography", Grade = "Grade 10", Topic = "Map Skills" },
        new { Subject = "Geography", Grade = "Grade 10", Topic = "Climate and Weather" },
        new { Subject = "Geography", Grade = "Grade 10", Topic = "Development Geography" },
        new { Subject = "Geography", Grade = "Grade 10", Topic = "Resources and Sustainability" },

        // =========================
        // GRADE 10 - LIFE ORIENTATION
        // =========================
        new { Subject = "Life Orientation", Grade = "Grade 10", Topic = "Personal Well-being" },
        new { Subject = "Life Orientation", Grade = "Grade 10", Topic = "Social Responsibility" },
        new { Subject = "Life Orientation", Grade = "Grade 10", Topic = "Physical Activity" },
        new { Subject = "Life Orientation", Grade = "Grade 10", Topic = "Study Skills" },
        new { Subject = "Life Orientation", Grade = "Grade 10", Topic = "Career Development" },
        new { Subject = "Life Orientation", Grade = "Grade 10", Topic = "Citizenship" },

        // =========================
        // GRADE 11 - MATHEMATICS
        // =========================
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Algebraic Expressions and Equations" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Sequences and Series" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Functions" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Analytical Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Trigonometry" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Statistics" },

        // =========================
        // GRADE 11 - ENGLISH
        // =========================
        new { Subject = "English", Grade = "Grade 11", Topic = "Critical Reading" },
        new { Subject = "English", Grade = "Grade 11", Topic = "Argumentative Writing" },
        new { Subject = "English", Grade = "Grade 11", Topic = "Language Structures" },
        new { Subject = "English", Grade = "Grade 11", Topic = "Transactional Writing" },
        new { Subject = "English", Grade = "Grade 11", Topic = "Poetry Analysis" },
        new { Subject = "English", Grade = "Grade 11", Topic = "Literary Texts" },

        // =========================
        // GRADE 11 - PHYSICAL SCIENCES
        // =========================
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Vectors and Scalars" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Newton's Laws" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Work, Energy and Power" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemical Bonding" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemical Reactions" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Electric Circuits" },

        // =========================
        // GRADE 11 - LIFE SCIENCES
        // =========================
        new { Subject = "Life Sciences", Grade = "Grade 11", Topic = "Biodiversity" },
        new { Subject = "Life Sciences", Grade = "Grade 11", Topic = "Plant Diversity" },
        new { Subject = "Life Sciences", Grade = "Grade 11", Topic = "Animal Diversity" },
        new { Subject = "Life Sciences", Grade = "Grade 11", Topic = "Human Transport Systems" },
        new { Subject = "Life Sciences", Grade = "Grade 11", Topic = "Human Respiratory System" },
        new { Subject = "Life Sciences", Grade = "Grade 11", Topic = "Human Excretion" },

        // =========================
        // GRADE 11 - GEOGRAPHY
        // =========================
        new { Subject = "Geography", Grade = "Grade 11", Topic = "Geomorphology" },
        new { Subject = "Geography", Grade = "Grade 11", Topic = "Climatology" },
        new { Subject = "Geography", Grade = "Grade 11", Topic = "Development Geography" },
        new { Subject = "Geography", Grade = "Grade 11", Topic = "Resources and Sustainability" },
        new { Subject = "Geography", Grade = "Grade 11", Topic = "Rural and Urban Settlements" },
        new { Subject = "Geography", Grade = "Grade 11", Topic = "Economic Geography" },

        // =========================
        // GRADE 11 - LIFE ORIENTATION
        // =========================
        new { Subject = "Life Orientation", Grade = "Grade 11", Topic = "Development of the Self" },
        new { Subject = "Life Orientation", Grade = "Grade 11", Topic = "Social and Environmental Responsibility" },
        new { Subject = "Life Orientation", Grade = "Grade 11", Topic = "Physical Activity" },
        new { Subject = "Life Orientation", Grade = "Grade 11", Topic = "Career Choices" },
        new { Subject = "Life Orientation", Grade = "Grade 11", Topic = "Democracy and Human Rights" },
        new { Subject = "Life Orientation", Grade = "Grade 11", Topic = "Study and Examination Skills" },

        // =========================
        // GRADE 12 - MATHEMATICS
        // =========================
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Functions" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Sequences and Series" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Financial Mathematics" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Probability" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Statistics" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Analytical Geometry" },

        // =========================
        // GRADE 12 - ENGLISH
        // =========================
        new { Subject = "English", Grade = "Grade 12", Topic = "Comprehension and Critical Reading" },
        new { Subject = "English", Grade = "Grade 12", Topic = "Essay Writing" },
        new { Subject = "English", Grade = "Grade 12", Topic = "Transactional Writing" },
        new { Subject = "English", Grade = "Grade 12", Topic = "Language in Context" },
        new { Subject = "English", Grade = "Grade 12", Topic = "Poetry" },
        new { Subject = "English", Grade = "Grade 12", Topic = "Literary Texts" },

        // =========================
        // GRADE 12 - PHYSICAL SCIENCES
        // =========================
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Momentum and Impulse" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Vertical Projectile Motion" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Work, Energy and Power" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Electrodynamics" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Chemical Equilibrium" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Organic Chemistry" },

        // =========================
        // GRADE 12 - LIFE SCIENCES
        // =========================
        new { Subject = "Life Sciences", Grade = "Grade 12", Topic = "DNA and Protein Synthesis" },
        new { Subject = "Life Sciences", Grade = "Grade 12", Topic = "Meiosis" },
        new { Subject = "Life Sciences", Grade = "Grade 12", Topic = "Genetics and Inheritance" },
        new { Subject = "Life Sciences", Grade = "Grade 12", Topic = "Evolution" },
        new { Subject = "Life Sciences", Grade = "Grade 12", Topic = "Human Reproduction" },
        new { Subject = "Life Sciences", Grade = "Grade 12", Topic = "Responding to the Environment" },

        // =========================
        // GRADE 12 - GEOGRAPHY
        // =========================
        new { Subject = "Geography", Grade = "Grade 12", Topic = "Fluvial Processes" },
        new { Subject = "Geography", Grade = "Grade 12", Topic = "Climate and Weather" },
        new { Subject = "Geography", Grade = "Grade 12", Topic = "Rural and Urban Settlement" },
        new { Subject = "Geography", Grade = "Grade 12", Topic = "Economic Geography of South Africa" },
        new { Subject = "Geography", Grade = "Grade 12", Topic = "Environmental Management" },
        new { Subject = "Geography", Grade = "Grade 12", Topic = "Mapwork" },

        // =========================
        // GRADE 12 - LIFE ORIENTATION
        // =========================
        new { Subject = "Life Orientation", Grade = "Grade 12", Topic = "Development of the Self in Society" },
        new { Subject = "Life Orientation", Grade = "Grade 12", Topic = "Social and Environmental Responsibility" },
        new { Subject = "Life Orientation", Grade = "Grade 12", Topic = "Democracy and Human Rights" },
        new { Subject = "Life Orientation", Grade = "Grade 12", Topic = "Careers and Career Choices" },
        new { Subject = "Life Orientation", Grade = "Grade 12", Topic = "Physical Activity" },
        new { Subject = "Life Orientation", Grade = "Grade 12", Topic = "Study and Examination Skills" }
    };

            foreach (var item in topics)
            {
                var subjectId = subjects[item.Subject];

                var exists = await context.Topics
                    .AnyAsync(t =>
                        t.SubjectID == subjectId &&
                        t.GradeLevel == item.Grade &&
                        t.TopicName == item.Topic);

                if (!exists)
                {
                    context.Topics.Add(new Topic
                    {
                        SubjectID = subjectId,
                        GradeLevel = item.Grade,
                        TopicName = item.Topic
                    });
                }
            }

            await context.SaveChangesAsync();
        }
        public static async Task SeedStudyMaterialsAsync(
    ApplicationDBContext context)
        {
            var topics = await context.Topics
                .Include(t => t.Subject)
                .ToListAsync();

            const string dbeSelfStudyGuidesUrl =
                "https://www.education.gov.za/SelfStudyGuidesGrade10-12.aspx";

            const string dbeTextbooksUrl =
                "https://www.education.gov.za/Curriculum/LearningandTeachingSupportMaterials%28LTSM%29/DigitalContent/StateOwnedTextbooksGrade10to12.aspx";

            const string dbeDigitalContentUrl =
                "https://www.education.gov.za/Curriculum/LearningandTeachingSupportMaterials%28LTSM%29/DigitalContent.aspx";

            foreach (var topic in topics)
            {
                var subjectName = topic.Subject?.SubjectName ?? "Subject";

                var materials = new[]
                {
            new StudyMaterial
            {
                TopicID = topic.TopicID,
                Title = $"{topic.TopicName} - DBE Study Guide",
                Description =
                    $"Official Department of Basic Education learning resources related to {topic.TopicName} in {subjectName}.",
                ResourceType = "Study Guide",
                FileURL = dbeSelfStudyGuidesUrl
            },

            new StudyMaterial
            {
                TopicID = topic.TopicID,
                Title = $"{topic.TopicName} - DBE Textbook Resources",
                Description =
                    $"Department of Basic Education textbook resources that can be used to support learning about {topic.TopicName}.",
                ResourceType = "Textbook",
                FileURL = dbeTextbooksUrl
            },

            new StudyMaterial
            {
                TopicID = topic.TopicID,
                Title = $"{topic.TopicName} - DBE Digital Resources",
                Description =
                    $"Additional Department of Basic Education digital learning resources for {topic.Subject?.SubjectName ?? "this subject"}.",
                ResourceType = "Reference",
                FileURL = dbeDigitalContentUrl
            }
        };

                foreach (var material in materials)
                {
                    var exists = await context.StudyMaterials
                        .AnyAsync(sm =>
                            sm.TopicID == material.TopicID &&
                            sm.Title == material.Title);

                    if (!exists)
                    {
                        context.StudyMaterials.Add(material);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
        public static async Task SeedDemoLearnersAsync(
    ApplicationDBContext context)
        {
            // Find the Learner role
            var learnerRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Learner");

            if (learnerRole == null)
            {
                throw new InvalidOperationException(
                    "Learner role has not been configured.");
            }

            // Find grades
            var grades = await context.Grades
                .ToDictionaryAsync(g => g.GradeName, g => g.GradeID);

            // Find schools
            var schools = await context.Schools
                .ToDictionaryAsync(s => s.SchoolName, s => s.SchoolID);

            var learners = new[]
            {
        // =========================
        // GRADE 10
        // =========================
        new
        {
            FirstName = "Thando",
            LastName = "Mbeki",
            Email = "thando.mbeki@eduquest.demo",
            Grade = "Grade 10",
            School = "AM Zantsi Senior Secondary School"
        },
        new
        {
            FirstName = "Amahle",
            LastName = "Ndlovu",
            Email = "amahle.ndlovu@eduquest.demo",
            Grade = "Grade 10",
            School = "Adam Masebe Secondary School"
        },
        new
        {
            FirstName = "Lwazi",
            LastName = "Jantjie",
            Email = "lwazi.jantjie@eduquest.demo",
            Grade = "Grade 10",
            School = "Fezekile Secondary School"
        },
        new
        {
            FirstName = "Naledi",
            LastName = "Mokoena",
            Email = "naledi.mokoena@eduquest.demo",
            Grade = "Grade 10",
            School = "Sehlabeng Secondary School"
        },
        new
        {
            FirstName = "Sibusiso",
            LastName = "Zulu",
            Email = "sibusiso.zulu@eduquest.demo",
            Grade = "Grade 10",
            School = "Abaqulusi High School"
        },
        new
        {
            FirstName = "Kagiso",
            LastName = "Molefe",
            Email = "kagiso.molefe@eduquest.demo",
            Grade = "Grade 10",
            School = "Areganeng Secondary School"
        },

        // =========================
        // GRADE 11
        // =========================
        new
        {
            FirstName = "Lukhanyo",
            LastName = "Nqoko",
            Email = "lukhanyo.nqoko@eduquest.demo",
            Grade = "Grade 11",
            School = "St Matthews High School"
        },
        new
        {
            FirstName = "Karabo",
            LastName = "Molefe",
            Email = "karabo.molefe@eduquest.demo",
            Grade = "Grade 11",
            School = "Bona Lesedi Secondary School"
        },
        new
        {
            FirstName = "Zintle",
            LastName = "Msimang",
            Email = "zintle.msimang@eduquest.demo",
            Grade = "Grade 11",
            School = "Garden Route High School"
        },
        new
        {
            FirstName = "Boitumelo",
            LastName = "Mokoena",
            Email = "boitumelo.mokoena@eduquest.demo",
            Grade = "Grade 11",
            School = "Sehunelo Secondary School"
        },
        new
        {
            FirstName = "Sipho",
            LastName = "Dlamini",
            Email = "sipho.dlamini@eduquest.demo",
            Grade = "Grade 11",
            School = "Bonga Secondary School"
        },
        new
        {
            FirstName = "Mpho",
            LastName = "Molefe",
            Email = "mpho.molefe@eduquest.demo",
            Grade = "Grade 11",
            School = "Badumedi Secondary School"
        },

        // =========================
        // GRADE 12
        // =========================
        new
        {
            FirstName = "Anele",
            LastName = "Nqana",
            Email = "anele.nqana@eduquest.demo",
            Grade = "Grade 12",
            School = "Emdemi Senior Secondary School"
        },
        new
        {
            FirstName = "Refilwe",
            LastName = "Mokoena",
            Email = "refilwe.mokoena@eduquest.demo",
            Grade = "Grade 12",
            School = "Cosmo City Secondary School"
        },
        new
        {
            FirstName = "Yamkela",
            LastName = "Mabena",
            Email = "yamkela.mabena@eduquest.demo",
            Grade = "Grade 12",
            School = "Fisantekraal High School"
        },
        new
        {
            FirstName = "Lesedi",
            LastName = "Molefe",
            Email = "lesedi.molefe@eduquest.demo",
            Grade = "Grade 12",
            School = "Teto Secondary School"
        },
        new
        {
            FirstName = "Andile",
            LastName = "Dlamini",
            Email = "andile.dlamini@eduquest.demo",
            Grade = "Grade 12",
            School = "Amazondi Secondary School"
        },
        new
        {
            FirstName = "Tshepo",
            LastName = "Mogale",
            Email = "tshepo.mogale@eduquest.demo",
            Grade = "Grade 12",
            School = "Potchefstroom High School for Boys"
        }
    };

            // One common password for demo accounts.
            // These are development/demo accounts only.
            const string demoPassword = "EduQuestDemo123!";

            foreach (var item in learners)
            {
                // Check whether this demo user already exists
                var existingUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == item.Email);

                if (existingUser != null)
                {
                    // Make sure the corresponding learner exists.
                    var learnerExists = await context.Learners
                        .AnyAsync(l => l.UserID == existingUser.UserID);

                    if (!learnerExists)
                    {
                        context.Learners.Add(new Learner
                        {
                            UserID = existingUser.UserID,
                            GradeID = grades[item.Grade],
                            SchoolID = schools[item.School]
                        });
                    }

                    continue;
                }

                // Create the User
                var user = new User
                {
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Email = item.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(demoPassword),
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    RoleID = learnerRole.RoleID
                };

                context.Users.Add(user);

                // Save first so UserID is generated
                await context.SaveChangesAsync();

                // Create the Learner linked to the User
                context.Learners.Add(new Learner
                {
                    UserID = user.UserID,
                    GradeID = grades[item.Grade],
                    SchoolID = schools[item.School]
                });

                await context.SaveChangesAsync();
            }
        }
        public static async Task SeedLearnerSubjectsAsync(ApplicationDBContext context)
        {
            var learners = await context.Learners
                .Where(l => l.User != null &&
                            l.User.Email.EndsWith("@eduquest.demo"))
                .ToListAsync();

            var subjects = await context.Subjects
                .ToListAsync();

            if (!learners.Any() || !subjects.Any())
                return;

            foreach (var learner in learners)
            {
                foreach (var subject in subjects)
                {
                    bool exists = await context.learnerSubjects
                        .AnyAsync(ls =>
                            ls.LearnerID == learner.LearnerID &&
                            ls.SubjectID == subject.SubjectID);

                    if (!exists)
                    {
                        context.learnerSubjects.Add(new LearnerSubject
                        {
                            LearnerID = learner.LearnerID,
                            SubjectID = subject.SubjectID
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}