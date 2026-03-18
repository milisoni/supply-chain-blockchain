using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SupplyChainApi.Models;

public class TransactionHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("transactionHash")]
    public string TransactionHash { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // CreateShipment, UpdateStatus, FundPayment, ReleasePayment

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("blockchainShipmentId")]
    public string? BlockchainShipmentId { get; set; }

    [BsonElement("blockchainAgreementId")]
    public string? BlockchainAgreementId { get; set; }

    [BsonElement("details")]
    public string? Details { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
