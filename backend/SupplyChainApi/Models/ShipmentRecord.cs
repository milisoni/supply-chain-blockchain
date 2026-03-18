using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SupplyChainApi.Models;

/// <summary>Off-chain shipment record linked to blockchain via TransactionHash and ShipmentId (bytes32).</summary>
public class ShipmentRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("blockchainShipmentId")]
    public string BlockchainShipmentId { get; set; } = string.Empty; // hex string of bytes32

    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("destination")]
    public string Destination { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Created"; // Created, Dispatched, InTransit, Delivered

    [BsonElement("transactionRef")]
    public string TransactionRef { get; set; } = string.Empty;

    [BsonElement("transactionHash")]
    public string? TransactionHash { get; set; }

    [BsonElement("createdByUserId")]
    public string CreatedByUserId { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
