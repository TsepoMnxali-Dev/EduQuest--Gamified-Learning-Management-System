using EduQuest.API.Data;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TokenService>();
builder.Services.AddHttpClient<GeminiQuizGeneratorService>();

builder.Services.AddScoped<IStatisticsService, StatisticsService>();

//JWT auth setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();
//end
//end

// CORS - allows the frontend (served from a different origin, e.g. a
// Live Server / http-server instance) to call this API from the browser.
// Add any other origin you serve the frontend from to this list.
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

var app = builder.Build();

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
    await DbSeeder.SeedStudyMaterialsAsync(context);
    await DbSeeder.SeedDemoLearnersAsync(context);
    await DbSeeder.SeedLearnerSubjectsAsync(context);

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("EduQuestFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();