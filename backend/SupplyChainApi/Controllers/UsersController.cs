using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SupplyChainApi.Data;
using SupplyChainApi.DTOs;
using SupplyChainApi.Models;

namespace SupplyChainApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly MongoDbContext _db;

    public UsersController(MongoDbContext db)
    {
        _db = db;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        if (UserId == null) return Unauthorized();
        var user = await _db.Users.Find(u => u.Id == UserId).FirstOrDefaultAsync(ct);
        if (user == null) return NotFound();
        return Ok(new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            BlockchainAddress = user.BlockchainAddress,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        });
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateRequest request, CancellationToken ct)
    {
        if (UserId == null) return Unauthorized();
        var updates = new List<UpdateDefinition<User>>();
        if (request.Name != null) updates.Add(Builders<User>.Update.Set(u => u.Name, request.Name));
        if (request.BlockchainAddress != null) updates.Add(Builders<User>.Update.Set(u => u.BlockchainAddress, request.BlockchainAddress));
        if (updates.Count == 0) return Ok();
        await _db.Users.UpdateOneAsync(
            Builders<User>.Filter.Eq(u => u.Id, UserId),
            Builders<User>.Update.Combine(updates),
            cancellationToken: ct);
        return Ok();
    }
}
