using System.ComponentModel.DataAnnotations;

namespace SupplyChainApi.DTOs;

public class CreatePaymentAgreementRequest
{
    [Required]
    public string BlockchainShipmentId { get; set; } = string.Empty;

    [Required]
    public string SupplierUserId { get; set; } = string.Empty;

    /// <summary>Amount in Wei (as string for large numbers).</summary>
    [Required]
    public string AmountWei { get; set; } = string.Empty;
}

public class ReleasePaymentRequest
{
    [Required]
    public string BlockchainAgreementId { get; set; } = string.Empty;
}

public class PaymentAgreementResponse
{
    public string Id { get; set; } = string.Empty;
    public string BlockchainAgreementId { get; set; } = string.Empty;
    public string BlockchainShipmentId { get; set; } = string.Empty;
    public string BuyerUserId { get; set; } = string.Empty;
    public string SupplierUserId { get; set; } = string.Empty;
    public string AmountWei { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}
