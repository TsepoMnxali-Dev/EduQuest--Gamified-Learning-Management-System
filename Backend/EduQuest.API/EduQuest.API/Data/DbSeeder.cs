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
        "Physical Sciences",
        "Accounting"
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
        "Physical Sciences",
        "Accounting"
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
        // =========================================================
        // GRADE 10 - MATHEMATICS
        // =========================================================
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Algebraic Expressions" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Exponents" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Number Patterns" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Equations and Inequalities" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Functions" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Finance and Growth" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Euclidean Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Trigonometry" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Analytical Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Statistics" },
        new { Subject = "Mathematics", Grade = "Grade 10", Topic = "Probability" },

        // =========================================================
        // GRADE 10 - PHYSICAL SCIENCES
        // =========================================================
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Physics - Waves, Sound and Light" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Physics - Electrostatics" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Physics - Electric Circuits" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Physics - Mechanics" },

        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Chemistry - Matter and Materials" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Chemistry - The Atom" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Chemistry - Chemical Bonding" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Chemistry - Chemical Change" },
        new { Subject = "Physical Sciences", Grade = "Grade 10", Topic = "Chemistry - The Hydrosphere" },

        // =========================================================
        // GRADE 10 - ACCOUNTING
        // =========================================================
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Accounting Concepts and GAAP Principles" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Bookkeeping of a Sole Trader" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Accounting Equation" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Ledgers and Trial Balance" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Value-Added Tax" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Salaries and Wages" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Year-End Adjustments and Final Accounts" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Financial Statements" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Analysis and Interpretation" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Reconciliations" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Cost Accounting" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Budgeting" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Ethics and Internal Control" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Fixed Assets and Inventory" },
        new { Subject = "Accounting", Grade = "Grade 10", Topic = "Indigenous Bookkeeping Systems" },

        // =========================================================
        // GRADE 11 - MATHEMATICS
        // =========================================================
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Exponents and Surds" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Equations and Inequalities" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Number Patterns" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Functions and Graphs" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Finance, Growth and Decay" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Probability" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Trigonometry" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Analytical Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Euclidean Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 11", Topic = "Statistics" },

        // =========================================================
        // GRADE 11 - PHYSICAL SCIENCES
        // =========================================================
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Physics - Vectors in Two Dimensions" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Physics - Newton's Laws" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Physics - Electrostatics" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Physics - Electric Circuits" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Physics - Electromagnetism" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Physics - Waves and the Doppler Effect" },

        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemistry - Atomic Combinations" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemistry - Intermolecular Forces" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemistry - Stoichiometry" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemistry - Energy and Chemical Change" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemistry - Acids and Bases" },
        new { Subject = "Physical Sciences", Grade = "Grade 11", Topic = "Chemistry - Ideal Gases" },

        // =========================================================
        // GRADE 11 - ACCOUNTING
        // =========================================================
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Partnerships" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Tangible Assets" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Reconciliations" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Analysis and Interpretation" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Value-Added Tax" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Inventory Systems" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Cost Accounting" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Budgeting" },
        new { Subject = "Accounting", Grade = "Grade 11", Topic = "Ethics and Internal Control" },

        // =========================================================
        // GRADE 12 - MATHEMATICS
        // =========================================================
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Patterns and Sequences" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Finance, Growth and Decay" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Functions and Graphs" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Algebra, Equations and Inequalities" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Differential Calculus" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Probability" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Euclidean Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Analytical Geometry" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Trigonometry" },
        new { Subject = "Mathematics", Grade = "Grade 12", Topic = "Statistics and Regression" },

        // =========================================================
        // GRADE 12 - PHYSICAL SCIENCES
        // =========================================================
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Mechanics" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Momentum and Impulse" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Vertical Projectile Motion" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Work, Energy and Power" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Doppler Effect" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Electrostatics" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Electric Circuits" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Electrodynamics" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Physics - Optical Phenomena and Properties of Matter" },

        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Chemistry - Organic Chemistry" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Chemistry - Rate and Extent of Reaction" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Chemistry - Chemical Equilibrium" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Chemistry - Acids and Bases" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Chemistry - Electrochemical Reactions" },
        new { Subject = "Physical Sciences", Grade = "Grade 12", Topic = "Chemistry - The Chemical Industry" },

        // =========================================================
        // GRADE 12 - ACCOUNTING
        // =========================================================
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Company Financial Statements and Notes" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Cash Flow Statements" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Financial Analysis and Interpretation" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Fixed Assets and Inventory Valuation" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Corporate Governance and Auditing" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "GAAP and IFRS Principles" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Manufacturing Concerns" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Cost Accounting and Break-Even Analysis" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Budgeting" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Reconciliations" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Value-Added Tax" },
        new { Subject = "Accounting", Grade = "Grade 12", Topic = "Internal Control" }
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

            var gradeSubjects = await context.GradeSubjects
                .Include(gs => gs.Grade)
                .Include(gs => gs.Subject)
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

                var gradeSubject = gradeSubjects.FirstOrDefault(gs =>
                    gs.SubjectID == topic.SubjectID &&
                    gs.Grade.GradeName == topic.GradeLevel);

                if (gradeSubject == null)
                {
                    continue;
                }

                var materials = new[]
                {
            new StudyMaterial
            {
                GradeSubjectID = gradeSubject.GradeSubjectID,
                TopicID = topic.TopicID,
                Title = $"{topic.TopicName} - DBE Study Guide",
                Description =
                    $"Official Department of Basic Education learning resources related to {topic.TopicName} in {subjectName}.",
                ResourceType = "Study Guide",
                FileURL = dbeSelfStudyGuidesUrl
            },

            new StudyMaterial
            {
                GradeSubjectID = gradeSubject.GradeSubjectID,
                TopicID = topic.TopicID,
                Title = $"{topic.TopicName} - DBE Textbook Resources",
                Description =
                    $"Department of Basic Education textbook resources that can be used to support learning about {topic.TopicName}.",
                ResourceType = "Textbook",
                FileURL = dbeTextbooksUrl
            },

            new StudyMaterial
            {
                GradeSubjectID = gradeSubject.GradeSubjectID,
                TopicID = topic.TopicID,
                Title = $"{topic.TopicName} - DBE Digital Resources",
                Description =
                    $"Additional Department of Basic Education digital learning resources for {subjectName}.",
                ResourceType = "Reference",
                FileURL = dbeDigitalContentUrl
            }
        };

                foreach (var material in materials)
                {
                    var exists = await context.StudyMaterials
                        .AnyAsync(sm =>
                            sm.GradeSubjectID == material.GradeSubjectID &&
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
        public static async Task SeedQuestionBankAsync(ApplicationDBContext context)
        {
            // Idempotency: don't reseed if bank already has data
            if (await context.QuestionBankItems.AnyAsync())
            {
                return;
            }

            var subjects = await context.Subjects
                .ToDictionaryAsync(s => s.SubjectName, s => s.SubjectID);

            var topics = await context.Topics.ToListAsync();

            int GetTopicId(string subjectName, string grade, string topicName)
            {
                var subjectId = subjects[subjectName];
                return topics
                    .First(t => t.SubjectID == subjectId
                             && t.GradeLevel == grade
                             && t.TopicName == topicName)
                    .TopicID;
            }

            var mathTopicId = GetTopicId("Mathematics", "Grade 12", "Differential Calculus");
            var physicsTopicId = GetTopicId("Physical Sciences", "Grade 12", "Physics - Electric Circuits");
            var accountingTopicId = GetTopicId("Accounting", "Grade 12", "Financial Analysis and Interpretation");

            var items = new List<QuestionBankItem>();

            items.AddRange(BuildDifferentialCalculusQuestions(mathTopicId));
            items.AddRange(BuildElectricCircuitsQuestions(physicsTopicId));
            items.AddRange(BuildFinancialAnalysisQuestions(accountingTopicId));

            context.QuestionBankItems.AddRange(items);
            await context.SaveChangesAsync();
        }

        private static List<QuestionBankItem> BuildDifferentialCalculusQuestions(int topicId)
        {
            return new List<QuestionBankItem>
    {
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "What is the derivative of f(x) = x\u00b3 using the power rule?",
            Explanation = "Power rule: d/dx[x^n] = n\u00b7x^(n-1), so the derivative of x\u00b3 is 3x\u00b2.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "3x\u00b2", IsCorrect = true },
                new() { OptionText = "x\u00b2", IsCorrect = false },
                new() { OptionText = "3x", IsCorrect = false },
                new() { OptionText = "x\u00b3/3", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "Using first principles, f'(x) is defined as:",
            Explanation = "The derivative from first principles is the limit of the difference quotient as h approaches 0.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "lim h\u21920 [f(x+h) - f(x)] / h", IsCorrect = true },
                new() { OptionText = "lim h\u21920 [f(x) - f(x+h)] / h", IsCorrect = false },
                new() { OptionText = "lim h\u21920 [f(x+h) + f(x)] / h", IsCorrect = false },
                new() { OptionText = "[f(x+h) - f(x)] / x", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "If f(x) = 5x\u00b2 - 3x + 7, what is f'(x)?",
            Explanation = "Differentiate term by term: d/dx[5x\u00b2]=10x, d/dx[-3x]=-3, d/dx[7]=0.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "10x - 3", IsCorrect = true },
                new() { OptionText = "10x + 3", IsCorrect = false },
                new() { OptionText = "5x - 3", IsCorrect = false },
                new() { OptionText = "10x\u00b2 - 3x", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "At a stationary point of a function, f'(x) equals:",
            Explanation = "Stationary points occur where the gradient (derivative) of the function is zero.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "0", IsCorrect = true },
                new() { OptionText = "1", IsCorrect = false },
                new() { OptionText = "Undefined", IsCorrect = false },
                new() { OptionText = "Infinity", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "What does the second derivative test determine at a stationary point?",
            Explanation = "If f''(x) > 0 the point is a local minimum; if f''(x) < 0 it is a local maximum.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Whether the point is a local maximum or minimum", IsCorrect = true },
                new() { OptionText = "The x-intercepts of the function", IsCorrect = false },
                new() { OptionText = "The y-intercept of the function", IsCorrect = false },
                new() { OptionText = "The domain of the function", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "The gradient of the tangent to a curve at point x = a is given by:",
            Explanation = "The derivative evaluated at x = a gives the gradient of the tangent line at that point.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "f'(a)", IsCorrect = true },
                new() { OptionText = "f(a)", IsCorrect = false },
                new() { OptionText = "f''(a)", IsCorrect = false },
                new() { OptionText = "f(a) / a", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "If f'(x) = 0 and f''(x) > 0 at x = a, the point (a, f(a)) is a:",
            Explanation = "A positive second derivative at a stationary point indicates the curve is concave up, i.e. a local minimum.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Local minimum", IsCorrect = true },
                new() { OptionText = "Local maximum", IsCorrect = false },
                new() { OptionText = "Point of inflection", IsCorrect = false },
                new() { OptionText = "Undefined point", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Hard",
            Source = "Manual",
            QuestionText = "A point of inflection occurs where:",
            Explanation = "Points of inflection occur where the second derivative is zero (or undefined) and the concavity of the function changes.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "f''(x) = 0 and concavity changes", IsCorrect = true },
                new() { OptionText = "f'(x) is undefined", IsCorrect = false },
                new() { OptionText = "f(x) = 0", IsCorrect = false },
                new() { OptionText = "f'(x) = f''(x)", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Hard",
            Source = "Manual",
            QuestionText = "Differential calculus can be used to determine the maximum volume of a box because it allows us to find where the:",
            Explanation = "Optimization problems use derivatives to find where the rate of change of a quantity equals zero, indicating a maximum or minimum.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Rate of change of volume with respect to a variable is zero", IsCorrect = true },
                new() { OptionText = "Volume is undefined", IsCorrect = false },
                new() { OptionText = "Box has the largest surface area", IsCorrect = false },
                new() { OptionText = "Derivative is undefined", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "What is the derivative of f(x) = 4x\u00b3 - 2x\u00b2 + x - 9?",
            Explanation = "Differentiate each term individually using the power rule.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "12x\u00b2 - 4x + 1", IsCorrect = true },
                new() { OptionText = "12x\u00b2 - 4x - 9", IsCorrect = false },
                new() { OptionText = "4x\u00b2 - 2x + 1", IsCorrect = false },
                new() { OptionText = "12x\u00b2 + 4x + 1", IsCorrect = false }
            }
        }
    };
        }

        private static List<QuestionBankItem> BuildElectricCircuitsQuestions(int topicId)
        {
            return new List<QuestionBankItem>
    {
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "According to Ohm's Law, the relationship between voltage (V), current (I), and resistance (R) is:",
            Explanation = "Ohm's Law states that voltage equals current multiplied by resistance.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "V = IR", IsCorrect = true },
                new() { OptionText = "V = I / R", IsCorrect = false },
                new() { OptionText = "V = R / I", IsCorrect = false },
                new() { OptionText = "I = VR", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "In a series circuit, the current through each component is:",
            Explanation = "In a series circuit there is only one path for current to flow, so the current is the same everywhere in the circuit.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "The same throughout the circuit", IsCorrect = true },
                new() { OptionText = "Different at each component", IsCorrect = false },
                new() { OptionText = "Zero", IsCorrect = false },
                new() { OptionText = "Dependent only on resistance", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "In a parallel circuit, the voltage across each branch is:",
            Explanation = "Each branch in a parallel circuit is connected directly across the same two nodes, so the voltage across each branch is equal.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "The same as the source voltage", IsCorrect = true },
                new() { OptionText = "Different for each branch", IsCorrect = false },
                new() { OptionText = "Always zero", IsCorrect = false },
                new() { OptionText = "The sum of all branch voltages", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "The total resistance of two resistors R1 and R2 connected in series is given by:",
            Explanation = "Resistances in series simply add together.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "R1 + R2", IsCorrect = true },
                new() { OptionText = "R1\u00d7R2 / (R1+R2)", IsCorrect = false },
                new() { OptionText = "1/R1 + 1/R2", IsCorrect = false },
                new() { OptionText = "R1 - R2", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "The total resistance of two resistors connected in parallel is calculated using:",
            Explanation = "For resistors in parallel, the reciprocal of the total resistance equals the sum of the reciprocals of each resistor.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "1/R_total = 1/R1 + 1/R2", IsCorrect = true },
                new() { OptionText = "R_total = R1 + R2", IsCorrect = false },
                new() { OptionText = "R_total = R1 - R2", IsCorrect = false },
                new() { OptionText = "R_total = R1 \u00d7 R2", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "The electromotive force (EMF) of a battery is defined as:",
            Explanation = "EMF is the total energy supplied per coulomb of charge by the source, including energy later lost to internal resistance.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "The total energy provided per unit charge by the battery", IsCorrect = true },
                new() { OptionText = "The voltage lost due to internal resistance", IsCorrect = false },
                new() { OptionText = "The current flowing through the battery", IsCorrect = false },
                new() { OptionText = "The resistance of the external circuit", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Hard",
            Source = "Manual",
            QuestionText = "The terminal voltage of a battery is less than its EMF because of:",
            Explanation = "Terminal voltage = EMF \u2212 (current \u00d7 internal resistance); some voltage is 'lost' driving current through the battery's own internal resistance.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Internal resistance causing a voltage drop inside the battery", IsCorrect = true },
                new() { OptionText = "External resistance being too high", IsCorrect = false },
                new() { OptionText = "The current being zero", IsCorrect = false },
                new() { OptionText = "The circuit being open", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "Electrical power dissipated in a resistor can be calculated using:",
            Explanation = "Power dissipated in a resistor equals current squared multiplied by resistance (derived from P = VI and V = IR).",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "P = I\u00b2R", IsCorrect = true },
                new() { OptionText = "P = I / R", IsCorrect = false },
                new() { OptionText = "P = R / I", IsCorrect = false },
                new() { OptionText = "P = IR\u00b2", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "If the external resistance in a circuit increases while EMF stays constant, the current in the circuit will:",
            Explanation = "By I = EMF / (R + r), increasing the external resistance R decreases the current.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Decrease", IsCorrect = true },
                new() { OptionText = "Increase", IsCorrect = false },
                new() { OptionText = "Stay the same", IsCorrect = false },
                new() { OptionText = "Become infinite", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Hard",
            Source = "Manual",
            QuestionText = "Kirchhoff's Current Law states that at any junction in a circuit:",
            Explanation = "This is conservation of charge \u2014 the total current flowing into a junction equals the total current flowing out.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "The sum of currents entering equals the sum of currents leaving", IsCorrect = true },
                new() { OptionText = "The sum of all voltages equals zero", IsCorrect = false },
                new() { OptionText = "Current is inversely proportional to resistance", IsCorrect = false },
                new() { OptionText = "Total power equals total energy", IsCorrect = false }
            }
        }
    };
        }

        private static List<QuestionBankItem> BuildFinancialAnalysisQuestions(int topicId)
        {
            return new List<QuestionBankItem>
    {
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "The current ratio is calculated as:",
            Explanation = "The current ratio measures liquidity by comparing current assets to current liabilities.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Current Assets : Current Liabilities", IsCorrect = true },
                new() { OptionText = "Current Liabilities : Current Assets", IsCorrect = false },
                new() { OptionText = "Fixed Assets : Current Liabilities", IsCorrect = false },
                new() { OptionText = "Current Assets : Fixed Liabilities", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "The acid test (quick) ratio differs from the current ratio because it:",
            Explanation = "The acid test ratio removes inventory, which is less liquid, to give a stricter measure of short-term liquidity.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Excludes inventory from current assets", IsCorrect = true },
                new() { OptionText = "Excludes cash from current assets", IsCorrect = false },
                new() { OptionText = "Includes fixed assets", IsCorrect = false },
                new() { OptionText = "Excludes current liabilities", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "Gross profit margin is calculated as:",
            Explanation = "Gross profit margin expresses gross profit as a percentage of sales revenue.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "(Gross Profit / Sales) \u00d7 100", IsCorrect = true },
                new() { OptionText = "(Net Profit / Sales) \u00d7 100", IsCorrect = false },
                new() { OptionText = "(Gross Profit / Cost of Sales) \u00d7 100", IsCorrect = false },
                new() { OptionText = "(Sales / Gross Profit) \u00d7 100", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "Return on equity (ROE) measures:",
            Explanation = "ROE = Net Profit / Average Shareholders' Equity \u00d7 100, showing the return generated on the owners' investment.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "How effectively a company uses shareholders' investment to generate profit", IsCorrect = true },
                new() { OptionText = "The total assets owned by a business", IsCorrect = false },
                new() { OptionText = "The amount of debt compared to equity", IsCorrect = false },
                new() { OptionText = "The liquidity of current assets", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "The debt-equity (gearing) ratio compares:",
            Explanation = "The gearing ratio shows how a business is financed \u2014 the proportion of debt versus equity.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Total external debt to shareholders' equity", IsCorrect = true },
                new() { OptionText = "Current assets to current liabilities", IsCorrect = false },
                new() { OptionText = "Gross profit to net profit", IsCorrect = false },
                new() { OptionText = "Sales to cost of sales", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "A high stock turnover rate generally indicates:",
            Explanation = "A high turnover rate shows efficient inventory management and a quick sales cycle.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Stock is being sold and replaced quickly", IsCorrect = true },
                new() { OptionText = "Stock is sitting unsold for long periods", IsCorrect = false },
                new() { OptionText = "The business has too much unsold inventory", IsCorrect = false },
                new() { OptionText = "The business is over-financed by debt", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Easy",
            Source = "Manual",
            QuestionText = "The debtors' collection period measures:",
            Explanation = "This ratio indicates how efficiently a business collects money owed by its customers.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "The average number of days it takes debtors to pay their accounts", IsCorrect = true },
                new() { OptionText = "The average number of days a business takes to pay creditors", IsCorrect = false },
                new() { OptionText = "The average number of days stock remains unsold", IsCorrect = false },
                new() { OptionText = "The ratio of debt to equity", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "Earnings per share (EPS) is calculated as:",
            Explanation = "EPS shows how much profit is attributable to each ordinary share in issue.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Net Profit after tax \u00f7 Number of issued shares", IsCorrect = true },
                new() { OptionText = "Net Profit before tax \u00f7 Number of issued shares", IsCorrect = false },
                new() { OptionText = "Dividends paid \u00f7 Number of issued shares", IsCorrect = false },
                new() { OptionText = "Total Equity \u00f7 Number of issued shares", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Medium",
            Source = "Manual",
            QuestionText = "A solvency ratio evaluates a company's ability to:",
            Explanation = "The solvency ratio (Total Assets : Total Liabilities) assesses whether a business can meet all its debts, not just short-term ones.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Meet its long-term financial obligations using total assets", IsCorrect = true },
                new() { OptionText = "Pay short-term debts using current assets only", IsCorrect = false },
                new() { OptionText = "Generate sales from its assets", IsCorrect = false },
                new() { OptionText = "Calculate profit margins", IsCorrect = false }
            }
        },
        new QuestionBankItem
        {
            TopicID = topicId,
            Difficulty = "Hard",
            Source = "Manual",
            QuestionText = "If a company's net profit margin decreases while its gross profit margin stays the same, this most likely indicates:",
            Explanation = "Since gross profit margin is unchanged, the drop in net profit margin points to higher costs below the gross profit line \u2014 operating expenses \u2014 rather than a change in cost of sales.",
            QuestionBankOptions = new List<QuestionBankOption>
            {
                new() { OptionText = "Operating expenses have increased relative to sales", IsCorrect = true },
                new() { OptionText = "Cost of sales has increased", IsCorrect = false },
                new() { OptionText = "Sales revenue has increased significantly", IsCorrect = false },
                new() { OptionText = "Gross profit has increased", IsCorrect = false }
            }
        }
    };
        }
    }
}