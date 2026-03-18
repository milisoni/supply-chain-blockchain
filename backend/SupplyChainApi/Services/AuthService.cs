using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SupplyChainApi.Configuration;
using SupplyChainApi.Data;
using SupplyChainApi.DTOs;
using SupplyChainApi.Models;
using MongoDB.Driver;

namespace SupplyChainApi.Services;

public class AuthService : IAuthService
{
    private readonly MongoDbContext _db;
    private readonly JwtSettings _jwt;

    public AuthService(MongoDbContext db, IOptions<JwtSettings> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _db.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync(ct);
        if (existing != null)
            return null;

        var role = NormalizeRole(request.Role);
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _db.Users.InsertOneAsync(user, cancellationToken: ct);

        return await LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password }, ct);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync(ct);
        if (user == null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwt(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            UserId = user.Id,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string NormalizeRole(string role)
    {
        var r = role.Trim();
        if (string.IsNullOrEmpty(r)) return "Supplier";
        var allowed = new[] { "Admin", "Supplier", "Manufacturer", "Transporter", "Distributor", "Retailer" };
        return allowed.Contains(r, StringComparer.OrdinalIgnoreCase) ? r : "Supplier";
    }
}
