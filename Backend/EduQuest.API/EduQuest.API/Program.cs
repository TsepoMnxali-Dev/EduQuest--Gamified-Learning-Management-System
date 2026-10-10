
using EduQuest.API.Data;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================================
// CONTROLLERS AND SWAGGER
// ==============================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        });
});

// ==============================================
// DATABASE CONNECTION
// ==============================================

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ==============================================
// APPLICATION SERVICES
// ==============================================

builder.Services.AddScoped<TokenService>();

builder.Services.AddHttpClient<GeminiQuizGeneratorService>();

builder.Services.AddScoped<IStatisticsService, StatisticsService>();

// ==============================================
// JWT AUTHENTICATION
// ==============================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!
            )
        )
    };
});

// Authorization checks roles such as Admin and Learner.
builder.Services.AddAuthorization();

// ==============================================
// CORS CONFIGURATION
// ==============================================

// Allows the frontend to communicate with the API.
// Add other trusted frontend origins if necessary.

builder.Services.AddCors(options =>
{
    options.AddPolicy("EduQuestFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5500",
                "http://127.0.0.1:5500",
                "http://localhost:5501",
                "http://127.0.0.1:5501",
                "http://localhost:3000",
                "http://localhost:5173"
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ==============================================
// BUILD APPLICATION
// ==============================================

var app = builder.Build();

// ==============================================
// DEVELOPMENT DATABASE SEEDING
// ==============================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDBContext>();

    var configuration = scope.ServiceProvider
        .GetRequiredService<IConfiguration>();

    // Create the default administrator.
    await DbSeeder.SeedAdminAsync(context, configuration);

    // Seed existing academic structure.
    // These are needed when the admin assigns
    // resources to grades, subjects and topics.

    await DbSeeder.SeedGradesAsync(context);

    await DbSeeder.SeedSubjectsAsync(context);

    await DbSeeder.SeedGradeSubjectsAsync(context);

    await DbSeeder.SeedTopicsAsync(context);

    // Existing quiz questions.
    await DbSeeder.SeedQuestionBankAsync(context);

    // ==========================================
    // CHANGED: DISABLE STUDY MATERIAL SEEDING
    // ==========================================

    // Previously:
    // await DbSeeder.SeedStudyMaterialsAsync(context);

    // NEW:
    // Study materials must only be added by
    // an administrator through the frontend.
    //
    // We do not want the database to automatically
    // contain sample documents or resource links.
    //
    // When the database has no study materials,
    // the learner Resources page will display:
    //
    // "No study materials available yet."
    //
    // The admin will later upload documents or
    // add learning links using the Resources form.

    // ==========================================
    // EXISTING LEARNER SEEDING
    // ==========================================

    await DbSeeder.SeedDemoLearnersAsync(context);

    await DbSeeder.SeedLearnerSubjectsAsync(context);

    // Enable Swagger in Development.
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==============================================
// HTTP REQUEST PIPELINE
// ==============================================

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Allow approved frontend origins.
app.UseCors("EduQuestFrontend");

// Authenticate users before checking permissions.
app.UseAuthentication();

// Apply authorization rules.
app.UseAuthorization();

// Map controller endpoints.
app.MapControllers();

// Start the application.
app.Run();
