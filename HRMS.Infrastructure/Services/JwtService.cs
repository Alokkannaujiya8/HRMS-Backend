using HRMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRMS.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string username, string role, int? employeeId = null, IReadOnlyCollection<string>? permissions = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            if (employeeId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, employeeId.Value.ToString()));
            }

            if (permissions != null)
            {
                claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
            }

            var keyValue = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.TryParse(_config["Jwt:DurationInMinutes"], out var minutes) ? minutes : 120),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
