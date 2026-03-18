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
public class ShipmentsController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly IBlockchainService _blockchain;

    public ShipmentsController(MongoDbContext db, IBlockchainService blockchain)
    {
        _db = db;
        _blockchain = blockchain;
    }

    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    /// <summary>Add a new shipment. Triggers smart contract and stores off-chain record.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request, CancellationToken ct)
    {
        if (UserId == null) return Unauthorized();

        var (shipmentIdHex, txHash) = await _blockchain.GetCreateShipmentResultAsync(
            request.ProductId, request.Quantity, request.Destination, request.TransactionRef ?? "", ct);

        if (shipmentIdHex == null && txHash == null)
            return BadRequest("Blockchain not configured or transaction failed. Ensure Ganache is running and contract addresses are set.");

        var statusName = "Created";
        var record = new ShipmentRecord
        {
            BlockchainShipmentId = shipmentIdHex ?? "",
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Destination = request.Destination,
            Status = statusName,
            TransactionRef = request.TransactionRef ?? "",
            TransactionHash = txHash,
            CreatedByUserId = UserId,
            CreatedAt = DateTime.UtcNow
        };
        await _db.Shipments.InsertOneAsync(record, ct);

        if (!string.IsNullOrEmpty(txHash))
        {
            await _db.TransactionHistory.InsertOneAsync(new TransactionHistory
            {
                TransactionHash = txHash,
                Type = "CreateShipment",
                UserId = UserId,
                BlockchainShipmentId = shipmentIdHex,
                Details = $"Product: {request.ProductId}, Qty: {request.Quantity}, Dest: {request.Destination}",
                CreatedAt = DateTime.UtcNow
            }, ct);
        }

        return CreatedAtAction(nameof(GetById), new { id = record.Id }, new ShipmentResponse
        {
            Id = record.Id,
            BlockchainShipmentId = record.BlockchainShipmentId,
            ProductId = record.ProductId,
            Quantity = record.Quantity,
            Destination = record.Destination,
            Status = record.Status,
            TransactionHash = record.TransactionHash,
            TransactionRef = record.TransactionRef,
            CreatedAt = record.CreatedAt
        });
    }

    /// <summary>Update shipment status (Dispatched=1, InTransit=2, Delivered=3). Authorized roles only.</summary>
    [HttpPut("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateShipmentStatusRequest request, CancellationToken ct)
    {
        if (UserId == null) return Unauthorized();

        var txHash = await _blockchain.UpdateShipmentStatusOnChainAsync(request.BlockchainShipmentId, request.Status, ct);
        if (string.IsNullOrEmpty(txHash))
            return BadRequest("Blockchain update failed.");

        var statusName = request.Status switch { 0 => "Created", 1 => "Dispatched", 2 => "InTransit", 3 => "Delivered", _ => "Unknown" };
        var filter = Builders<ShipmentRecord>.Filter.Eq(s => s.BlockchainShipmentId, request.BlockchainShipmentId);
        var update = Builders<ShipmentRecord>.Update.Combine(
            Builders<ShipmentRecord>.Update.Set(s => s.Status, statusName),
            Builders<ShipmentRecord>.Update.Set(s => s.UpdatedAt, DateTime.UtcNow));
        await _db.Shipments.UpdateOneAsync(filter, update, cancellationToken: ct);

        await _db.TransactionHistory.InsertOneAsync(new TransactionHistory
        {
            TransactionHash = txHash,
            Type = "UpdateStatus",
            UserId = UserId,
            BlockchainShipmentId = request.BlockchainShipmentId,
            Details = $"Status: {statusName}",
            CreatedAt = DateTime.UtcNow
        }, ct);

        return Ok(new { transactionHash = txHash, status = statusName });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var record = await _db.Shipments.Find(s => s.Id == id).FirstOrDefaultAsync(ct);
        if (record == null) return NotFound();
        return Ok(ToResponse(record));
    }

    /// <summary>List shipments (optionally by current user).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] bool myOnly = false, CancellationToken ct = default)
    {
        var query = myOnly && UserId != null
            ? _db.Shipments.Find(s => s.CreatedByUserId == UserId)
            : _db.Shipments.Find(_ => true);
        var list = await query.SortByDescending(s => s.CreatedAt).ToListAsync(ct);
        return Ok(list.Select(ToResponse).ToList());
    }

    /// <summary>Track shipment by blockchain ID. Returns current status from chain and off-chain details.</summary>
    [HttpGet("track/{blockchainShipmentId}")]
    [ProducesResponseType(typeof(TrackShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Track(string blockchainShipmentId, CancellationToken ct)
    {
        var record = await _db.Shipments.Find(s => s.BlockchainShipmentId == blockchainShipmentId).FirstOrDefaultAsync(ct);
        var chainStatus = await _blockchain.GetShipmentStatusFromChainAsync(blockchainShipmentId, ct);
        var statusName = record?.Status ?? (chainStatus.HasValue ? BlockchainService.GetStatusName(chainStatus.Value) : "Unknown");
        if (record != null)
        {
            if (chainStatus.HasValue)
                statusName = BlockchainService.GetStatusName(chainStatus.Value);
            return Ok(new TrackShipmentResponse
            {
                BlockchainShipmentId = record.BlockchainShipmentId,
                ProductId = record.ProductId,
                Quantity = record.Quantity,
                Destination = record.Destination,
                Status = statusName,
                TransactionHash = record.TransactionHash,
                CreatedAt = record.CreatedAt
            });
        }
        if (chainStatus.HasValue)
            return Ok(new TrackShipmentResponse
            {
                BlockchainShipmentId = blockchainShipmentId,
                ProductId = "",
                Quantity = 0,
                Destination = "",
                Status = BlockchainService.GetStatusName(chainStatus.Value),
                TransactionHash = null,
                CreatedAt = DateTime.MinValue
            });
        return NotFound();
    }

    private static ShipmentResponse ToResponse(ShipmentRecord r) => new()
    {
        Id = r.Id,
        BlockchainShipmentId = r.BlockchainShipmentId,
        ProductId = r.ProductId,
        Quantity = r.Quantity,
        Destination = r.Destination,
        Status = r.Status,
        TransactionHash = r.TransactionHash,
        TransactionRef = r.TransactionRef,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
