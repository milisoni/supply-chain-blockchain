using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SupplyChainApi.Models;

public class PaymentAgreementRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("blockchainAgreementId")]
    public string BlockchainAgreementId { get; set; } = string.Empty;

    [BsonElement("blockchainShipmentId")]
    public string BlockchainShipmentId { get; set; } = string.Empty;

    [BsonElement("buyerUserId")]
    public string BuyerUserId { get; set; } = string.Empty;

    [BsonElement("supplierUserId")]
    public string SupplierUserId { get; set; } = string.Empty;

    [BsonElement("amountWei")]
    public string AmountWei { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Funded"; // Funded, Released, Refunded

    [BsonElement("transactionHash")]
    public string? TransactionHash { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("releasedAt")]
    public DateTime? ReleasedAt { get; set; }
}
