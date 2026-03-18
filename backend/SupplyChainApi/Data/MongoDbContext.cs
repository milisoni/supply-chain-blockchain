using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SupplyChainApi.Configuration;
using SupplyChainApi.Models;

namespace SupplyChainApi.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<ShipmentRecord> Shipments => _database.GetCollection<ShipmentRecord>("Shipments");
    public IMongoCollection<TransactionHistory> TransactionHistory => _database.GetCollection<TransactionHistory>("TransactionHistory");
    public IMongoCollection<PaymentAgreementRecord> PaymentAgreements => _database.GetCollection<PaymentAgreementRecord>("PaymentAgreements");
}
