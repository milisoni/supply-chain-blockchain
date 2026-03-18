using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SupplyChainApi.Data;
using SupplyChainApi.DTOs;
using SupplyChainApi.Models;

namespace SupplyChainApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly MongoDbContext _db;

    public AdminController(MongoDbContext db)
    {
        _db = db;
    }

    /// <summary>List all users.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUsers(CancellationToken ct)
    {
        var list = await _db.Users.Find(_ => true).SortBy(u => u.Email).ToListAsync(ct);
        return Ok(list.Select(u => new UserResponse
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            BlockchainAddress = u.BlockchainAddress,
            CreatedAt = u.CreatedAt,
            IsActive = u.IsActive
        }).ToList());
    }

    /// <summary>Get user by ID.</summary>
    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string id, CancellationToken ct)
    {
        var user = await _db.Users.Find(u => u.Id == id).FirstOrDefaultAsync(ct);
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

    /// <summary>Update user (name, blockchain address, isActive).</summary>
    [HttpPut("users/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateRequest request, CancellationToken ct)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, id);
        var updates = new List<UpdateDefinition<User>>();
        if (request.Name != null) updates.Add(Builders<User>.Update.Set(u => u.Name, request.Name));
        if (request.BlockchainAddress != null) updates.Add(Builders<User>.Update.Set(u => u.BlockchainAddress, request.BlockchainAddress));
        if (request.IsActive.HasValue) updates.Add(Builders<User>.Update.Set(u => u.IsActive, request.IsActive.Value));
        if (updates.Count == 0) return Ok();
        var update = Builders<User>.Update.Combine(updates);
        var result = await _db.Users.UpdateOneAsync(filter, update, cancellationToken: ct);
        if (result.MatchedCount == 0) return NotFound();
        return Ok();
    }

    /// <summary>Overview: counts of users, shipments, transactions.</summary>
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Overview(CancellationToken ct)
    {
        var usersCount = await _db.Users.CountDocumentsAsync(_ => true, cancellationToken: ct);
        var shipmentsCount = await _db.Shipments.CountDocumentsAsync(_ => true, cancellationToken: ct);
        var transactionsCount = await _db.TransactionHistory.CountDocumentsAsync(_ => true, cancellationToken: ct);
        var paymentsCount = await _db.PaymentAgreements.CountDocumentsAsync(_ => true, cancellationToken: ct);
        return Ok(new
        {
            usersCount,
            shipmentsCount,
            transactionsCount,
            paymentsCount
        });
    }
}
