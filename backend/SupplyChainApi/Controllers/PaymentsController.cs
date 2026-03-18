using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SupplyChainApi.Data;
using SupplyChainApi.DTOs;
using SupplyChainApi.Models;
using SupplyChainApi.Services;

namespace SupplyChainApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly IBlockchainService _blockchain;

    public PaymentsController(MongoDbContext db, IBlockchainService blockchain)
    {
        _db = db;
        _blockchain = blockchain;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    /// <summary>Create and fund a payment agreement (escrow) for a shipment. Supplier must have a blockchain address.</summary>
    [HttpPost("fund")]
    [ProducesResponseType(typeof(PaymentAgreementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FundAgreement([FromBody] CreatePaymentAgreementRequest request, CancellationToken ct)
    {
        if (UserId == null) return Unauthorized();

        var supplier = await _db.Users.Find(u => u.Id == request.SupplierUserId).FirstOrDefaultAsync(ct);
        if (supplier?.BlockchainAddress == null)
            return BadRequest("Supplier user not found or has no blockchain address.");

        if (!decimal.TryParse(request.AmountWei, System.Globalization.NumberStyles.Integer, null, out var amountWei) || amountWei <= 0)
            return BadRequest("Invalid amount (Wei).");

        var fundResult = await _blockchain.FundPaymentAgreementAsync(
            request.BlockchainShipmentId, supplier.BlockchainAddress, amountWei, ct);
        var agreementIdHex = fundResult.agreementIdHex;
        var txHash = fundResult.txHash;

        if (string.IsNullOrEmpty(txHash))
            return BadRequest("Blockchain payment funding failed. Check RPC and contract config.");

        var record = new PaymentAgreementRecord
        {
            BlockchainAgreementId = agreementIdHex ?? $"{request.BlockchainShipmentId}-{UserId}-{DateTime.UtcNow.Ticks}",
            BlockchainShipmentId = request.BlockchainShipmentId,
            BuyerUserId = UserId,
            SupplierUserId = request.SupplierUserId,
            AmountWei = request.AmountWei,
            Status = "Funded",
            TransactionHash = txHash,
            CreatedAt = DateTime.UtcNow
        };
        await _db.PaymentAgreements.InsertOneAsync(record, ct);
        await _db.TransactionHistory.InsertOneAsync(new TransactionHistory
        {
            TransactionHash = txHash,
            Type = "FundPayment",
            UserId = UserId,
            BlockchainShipmentId = request.BlockchainShipmentId,
            BlockchainAgreementId = record.BlockchainAgreementId,
            Details = $"Amount: {request.AmountWei} Wei",
            CreatedAt = DateTime.UtcNow
        }, ct);

        return CreatedAtAction(nameof(GetAgreement), new { id = record.Id }, new PaymentAgreementResponse
        {
            Id = record.Id,
            BlockchainAgreementId = record.BlockchainAgreementId,
            BlockchainShipmentId = record.BlockchainShipmentId,
            BuyerUserId = record.BuyerUserId,
            SupplierUserId = record.SupplierUserId,
            AmountWei = record.AmountWei,
            Status = record.Status,
            TransactionHash = record.TransactionHash,
            CreatedAt = record.CreatedAt
        });
    }

    /// <summary>Confirm delivery and release payment to supplier. Only succeeds when shipment status is Delivered on chain.</summary>
    [HttpPost("release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReleasePayment([FromBody] ReleasePaymentRequest request, CancellationToken ct)
    {
        if (UserId == null) return Unauthorized();

        var txHash = await _blockchain.ConfirmDeliveryAndReleasePaymentAsync(request.BlockchainAgreementId, ct);
        if (string.IsNullOrEmpty(txHash))
            return BadRequest("Release failed. Ensure shipment is Delivered on blockchain and agreement exists.");

        var filter = Builders<PaymentAgreementRecord>.Filter.Eq(p => p.BlockchainAgreementId, request.BlockchainAgreementId);
        var update = Builders<PaymentAgreementRecord>.Update.Combine(
            Builders<PaymentAgreementRecord>.Update.Set(p => p.Status, "Released"),
            Builders<PaymentAgreementRecord>.Update.Set(p => p.ReleasedAt, DateTime.UtcNow));
        await _db.PaymentAgreements.UpdateOneAsync(filter, update, cancellationToken: ct);
        await _db.TransactionHistory.InsertOneAsync(new TransactionHistory
        {
            TransactionHash = txHash,
            Type = "ReleasePayment",
            UserId = UserId,
            BlockchainAgreementId = request.BlockchainAgreementId,
            Details = "Payment released to supplier",
            CreatedAt = DateTime.UtcNow
        }, ct);

        return Ok(new { transactionHash = txHash, status = "Released" });
    }

    [HttpGet("agreement/{id}")]
    [ProducesResponseType(typeof(PaymentAgreementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAgreement(string id, CancellationToken ct)
    {
        var record = await _db.PaymentAgreements.Find(p => p.Id == id).FirstOrDefaultAsync(ct);
        if (record == null) return NotFound();
        return Ok(ToResponse(record));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentAgreementResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] bool myOnly = true, CancellationToken ct = default)
    {
        var query = myOnly && UserId != null
            ? _db.PaymentAgreements.Find(p => p.BuyerUserId == UserId || p.SupplierUserId == UserId)
            : _db.PaymentAgreements.Find(_ => true);
        var list = await query.SortByDescending(p => p.CreatedAt).ToListAsync(ct);
        return Ok(list.Select(ToResponse).ToList());
    }

    private static PaymentAgreementResponse ToResponse(PaymentAgreementRecord r) => new()
    {
        Id = r.Id,
        BlockchainAgreementId = r.BlockchainAgreementId,
        BlockchainShipmentId = r.BlockchainShipmentId,
        BuyerUserId = r.BuyerUserId,
        SupplierUserId = r.SupplierUserId,
        AmountWei = r.AmountWei,
        Status = r.Status,
        TransactionHash = r.TransactionHash,
        CreatedAt = r.CreatedAt,
        ReleasedAt = r.ReleasedAt
    };
}
