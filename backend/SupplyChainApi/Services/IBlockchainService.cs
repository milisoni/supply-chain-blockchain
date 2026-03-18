namespace SupplyChainApi.Services;

public interface IBlockchainService
{
    Task<bool> IsConfiguredAsync(CancellationToken ct = default);
    Task<string?> CreateShipmentOnChainAsync(string productId, int quantity, string destination, string transactionRef, CancellationToken ct = default);
    Task<string?> UpdateShipmentStatusOnChainAsync(string shipmentIdHex, int status, CancellationToken ct = default);
    Task<(string? shipmentIdHex, string? txHash)> GetCreateShipmentResultAsync(string productId, int quantity, string destination, string transactionRef, CancellationToken ct = default);
    Task<int?> GetShipmentStatusFromChainAsync(string shipmentIdHex, CancellationToken ct = default);
    Task<(string? productId, int? quantity, string? destination, uint? createdAt, int? status)> GetShipmentFromChainAsync(string shipmentIdHex, CancellationToken ct = default);
    Task<(string? agreementIdHex, string? txHash)> FundPaymentAgreementAsync(string shipmentIdHex, string supplierAddress, decimal amountWei, CancellationToken ct = default);
    Task<string?> ConfirmDeliveryAndReleasePaymentAsync(string agreementIdHex, CancellationToken ct = default);
    Task<int?> GetPaymentAgreementStatusAsync(string agreementIdHex, CancellationToken ct = default);
}
