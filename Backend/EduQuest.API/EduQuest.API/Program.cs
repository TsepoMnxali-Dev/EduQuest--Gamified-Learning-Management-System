
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
builder.Services.AddScoped<IEmailService, EmailService>();
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

builder.Services.AddAuthorization();

// ==============================================
// CORS CONFIGURATION
// ==============================================
// Development: Allow local frontend addresses.
// Production: Read permitted origins from
// configuration without changing this file.

var allowedOrigins = new List<string>();

if (builder.Environment.IsDevelopment())
{
    allowedOrigins.AddRange(new[]
    {
        "http://localhost:5500",
        "http://127.0.0.1:5500",
        "http://localhost:5501",
        "http://127.0.0.1:5501",
        "http://localhost:3000",
        "http://localhost:5173"
    });
}

var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

allowedOrigins.AddRange(
    configuredOrigins
        .Where(origin => !string.IsNullOrWhiteSpace(origin))
        .Select(origin => origin.Trim().TrimEnd('/'))
);

var finalOrigins = allowedOrigins
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("EduQuestFrontend", policy =>
    {
        policy.WithOrigins(finalOrigins)
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

    await DbSeeder.SeedAdminAsync(context, configuration);

    await DbSeeder.SeedGradesAsync(context);
    await DbSeeder.SeedSubjectsAsync(context);
    await DbSeeder.SeedGradeSubjectsAsync(context);
    await DbSeeder.SeedTopicsAsync(context);

    await DbSeeder.SeedQuestionBankAsync(context);

    // Study materials are only added by administrators.
    // await DbSeeder.SeedStudyMaterialsAsync(context);

    await DbSeeder.SeedDemoLearnersAsync(context);
    await DbSeeder.SeedLearnerSubjectsAsync(context);

    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==============================================
// HTTP REQUEST PIPELINE
// ==============================================

app.UseHttpsRedirection();

// Serve EduQuest HTML, CSS, JavaScript and images.
app.UseStaticFiles();

app.UseCors("EduQuestFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Existing API endpoints.
app.MapControllers();

// Open the learner homepage at the application root.
// PathBase also supports an IIS application subdirectory.
app.MapGet("/", (HttpContext context) =>
    Results.Redirect(
        $"{context.Request.PathBase}/EduQuest-with-js/EduQuest/index.html"
    )
);

app.Run();
