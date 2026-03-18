namespace SupplyChainApi.DTOs;

public class TransactionHistoryItem
{
    public string Id { get; set; } = string.Empty;
    public string TransactionHash { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? BlockchainShipmentId { get; set; }
    public string? BlockchainAgreementId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
