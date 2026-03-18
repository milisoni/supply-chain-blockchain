using System.ComponentModel.DataAnnotations;

namespace SupplyChainApi.DTOs;

public class CreateShipmentRequest
{
    [Required, MaxLength(200)]
    public string ProductId { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required, MaxLength(500)]
    public string Destination { get; set; } = string.Empty;

    [MaxLength(200)]
    public string TransactionRef { get; set; } = string.Empty;
}

public class UpdateShipmentStatusRequest
{
    [Required]
    public string BlockchainShipmentId { get; set; } = string.Empty;

    /// <summary>Created=0, Dispatched=1, InTransit=2, Delivered=3</summary>
    [Required, Range(0, 3)]
    public int Status { get; set; }
}

public class ShipmentResponse
{
    public string Id { get; set; } = string.Empty;
    public string BlockchainShipmentId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionHash { get; set; }
    public string TransactionRef { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TrackShipmentResponse
{
    public string BlockchainShipmentId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
