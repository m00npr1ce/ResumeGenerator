using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ResumeGenerator.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(string secretKey, string issuer, string audience)
        {
            _secretKey = secretKey;
            _issuer = issuer;
            _audience = audience;
        }

        public string GenerateToken(string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, email),
                // Можно добавить другие claims, например, для ролей
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
