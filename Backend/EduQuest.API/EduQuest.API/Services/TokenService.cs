
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduQuest.API.Models.Entities;

namespace EduQuest.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(User user)
        {
            // NEW:
            // A user must have a valid role
            // before a JWT token is generated.
            if (user.Role == null ||
                string.IsNullOrWhiteSpace(user.Role.RoleName))
            {
                throw new InvalidOperationException(
                    "Cannot generate a token without a valid user role.");
            }

            // ==========================================
            // JWT CLAIMS
            // ==========================================

            var claims = new List<Claim>
            {
                // User's unique ID.
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserID.ToString()),

                // User's email address.
                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                // CHANGED:
                // Use the actual role from the database.
                // Never automatically assign Learner.
                new Claim(
                    ClaimTypes.Role,
                    user.Role.RoleName)
            };

            // ==========================================
            // SIGNING KEY
            // ==========================================

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // ==========================================
            // GENERATE JWT
            // ==========================================

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(
                        _config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
