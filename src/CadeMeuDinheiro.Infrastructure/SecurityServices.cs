using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CadeMeuDinheiro.Application;
using CadeMeuDinheiro.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CadeMeuDinheiro.Infrastructure;

public sealed class PasswordService : IPasswordService
{
    private const int Iterations = 210_000;
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, 32);
        return $"pbkdf2-sha512${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations)) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[2]); var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
    }
}

public sealed class JwtOptions
{
    public const string Section = "Jwt";
    public string Issuer { get; init; } = "CadeMeuDinheiro";
    public string Audience { get; init; } = "CadeMeuDinheiro.App";
    public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
}

public sealed class TokenService(IOptions<JwtOptions> options, TimeProvider clock) : ITokenService
{
    public AuthTokens Create(User user)
    {
        var settings = options.Value;
        if (Encoding.UTF8.GetByteCount(settings.SigningKey) < 32) throw new InvalidOperationException("JWT signing key must contain at least 32 bytes.");
        var now = clock.GetUtcNow(); var expires = now.AddMinutes(settings.AccessTokenMinutes);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new(JwtRegisteredClaimNames.Email, user.Email), new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())]),
            Issuer = settings.Issuer, Audience = settings.Audience, IssuedAt = now.UtcDateTime, Expires = expires.UtcDateTime,
            SigningCredentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)), SecurityAlgorithms.HmacSha256)
        };
        var handler = new JwtSecurityTokenHandler();
        return new(handler.WriteToken(handler.CreateToken(descriptor)), expires);
    }
}
