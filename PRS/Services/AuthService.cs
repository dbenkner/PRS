using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PRS.Models;
namespace PRS.Services
{
    public class AuthService
    {
        private readonly string _secretKey;

        public AuthService(string secretKey) { 
            _secretKey = secretKey;
        }
        public string Create(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var privateKey = Encoding.UTF8.GetBytes(_secretKey);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(privateKey),
                SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddHours(1),
                Subject = GenerateClaims(user)
            };
            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        private static ClaimsIdentity GenerateClaims(User user)
        {
            var ci = new ClaimsIdentity();
            ci.AddClaim(new Claim("id", user.Id.ToString()));
            ci.AddClaim(new Claim(ClaimTypes.Name, user.Username));


            if(user.UserRoles != null)
            {
                foreach (var role in user.UserRoles)
                {
                    ci.AddClaim(new Claim(ClaimTypes.Role, role.Role.Rolename));
                }
            }
            return ci;
        }
    }
}
