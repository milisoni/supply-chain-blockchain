using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupplyChainApi.Data;
using SupplyChainApi.DTOs;
using MongoDB.Driver;

namespace SupplyChainApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionHistoryController : ControllerBase
{
    private readonly MongoDbContext _db;

    public TransactionHistoryController(MongoDbContext db)
    {
        _db = db;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    /// <summary>Get transaction history. Admin sees all; others see their own.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionHistoryItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var isAdmin = User.IsInRole("Admin");
        var query = isAdmin
            ? _db.TransactionHistory.Find(_ => true)
            : _db.TransactionHistory.Find(t => t.UserId == UserId);
        var list = await query.SortByDescending(t => t.CreatedAt).Limit(limit).ToListAsync(ct);
        return Ok(list.Select(t => new TransactionHistoryItem
        {
            Id = t.Id,
            TransactionHash = t.TransactionHash,
            Type = t.Type,
            UserId = t.UserId,
            BlockchainShipmentId = t.BlockchainShipmentId,
            BlockchainAgreementId = t.BlockchainAgreementId,
            Details = t.Details,
            CreatedAt = t.CreatedAt
        }).ToList());
    }
}
