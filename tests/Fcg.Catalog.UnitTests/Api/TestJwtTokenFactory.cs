using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Fcg.Catalog.UnitTests.Api;

internal static class TestJwtTokenFactory
{
    private const string Issuer = "fcg-users-api";
    private const string Audience = "fcg-services";
    private const string SecretKey = "development-secret-key-change-before-production";

    public static string CreateUserToken(Guid? userId = null)
    {
        return CreateToken(userId ?? Guid.NewGuid(), "User");
    }

    public static string CreateAdministratorToken(Guid? userId = null)
    {
        return CreateToken(userId ?? Guid.NewGuid(), "Administrator");
    }

    private static string CreateToken(Guid userId, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            Issuer,
            Audience,
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, $"{userId:N}@example.com"),
                new Claim(ClaimTypes.Role, role)
            ],
            now,
            now.AddHours(1),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
