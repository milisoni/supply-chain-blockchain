using System.Numerics;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json.Linq;
using SupplyChainApi.Abi;
using SupplyChainApi.Configuration;
using SupplyChainApi.Services;

namespace SupplyChainApi.Services;

public class BlockchainService : IBlockchainService
{
    private readonly BlockchainSettings _settings;
    private Web3? _web3;
    private bool _initialized;

    private static readonly string[] StatusNames = { "Created", "Dispatched", "InTransit", "Delivered" };
    private static readonly string[] PaymentStatusNames = { "Pending", "Funded", "Delivered", "Released", "Refunded" };

    public BlockchainService(IOptions<BlockchainSettings> settings)
    {
        _settings = settings.Value;
    }

    private void EnsureWeb3()
    {
        if (_initialized) return;
        if (string.IsNullOrWhiteSpace(_settings.RpcUrl))
        {
            _initialized = true;
            return;
        }

        if (!string.IsNullOrWhiteSpace(_settings.AccountPrivateKey))
        {
            var key = _settings.AccountPrivateKey.Trim();
            if (!key.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                key = "0x" + key;
            var account = new Account(key);
            _web3 = new Web3(account, _settings.RpcUrl);
        }
        else
            _web3 = new Web3(_settings.RpcUrl);

        _initialized = true;
    }

    public Task<bool> IsConfiguredAsync(CancellationToken ct = default)
    {
        EnsureWeb3();
        var ok = _web3 != null
                 && !string.IsNullOrWhiteSpace(_settings.SupplyChainContractAddress)
                 && !string.IsNullOrWhiteSpace(_settings.RpcUrl);
        return Task.FromResult(ok);
    }

    private static byte[] HexToBytes32(string hex)
    {
        hex = hex.Trim();
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex[2..];
        if (hex.Length > 64) hex = hex[^64..];
        while (hex.Length < 64) hex = "0" + hex;
        var bytes = new byte[32];
        for (var i = 0; i < 32; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }

    private static string Bytes32ToHex(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return "0x";
        var b = bytes.Length >= 32 ? bytes.Take(32).ToArray() : bytes;
        return "0x" + Convert.ToHexString(b).ToLowerInvariant();
    }

    public async Task<string?> CreateShipmentOnChainAsync(string productId, int quantity, string destination, string transactionRef, CancellationToken ct = default)
    {
        EnsureWeb3();
        if (_web3 == null || string.IsNullOrEmpty(_settings.SupplyChainContractAddress))
            return null;

        var contract = _web3.Eth.GetContract(SupplyChainAbi.Full, _settings.SupplyChainContractAddress);
        var createFn = contract.GetFunction("createShipment");
        var txHash = await createFn.SendTransactionAsync(
            _web3.TransactionManager.Account.Address,
            new HexBigInteger(3_000_000),
            null,
            productId,
            new BigInteger(quantity),
            destination,
            transactionRef ?? "").ConfigureAwait(false);

        return txHash;
    }

    public async Task<string?> UpdateShipmentStatusOnChainAsync(string shipmentIdHex, int status, CancellationToken ct = default)
    {
        EnsureWeb3();
        if (_web3 == null || string.IsNullOrEmpty(_settings.SupplyChainContractAddress))
            return null;

        var bytes32 = HexToBytes32(shipmentIdHex);
        var contract = _web3.Eth.GetContract(SupplyChainAbi.Full, _settings.SupplyChainContractAddress);
        var fn = contract.GetFunction("updateShipmentStatus");
        var txHash = await fn.SendTransactionAsync(
            _web3.TransactionManager.Account.Address,
            new HexBigInteger(500_000),
            null,
            bytes32,
            (byte)status).ConfigureAwait(false);
        return txHash;
    }

    public async Task<(string? shipmentIdHex, string? txHash)> GetCreateShipmentResultAsync(string productId, int quantity, string destination, string transactionRef, CancellationToken ct = default)
    {
        var txHash = await CreateShipmentOnChainAsync(productId, quantity, destination, transactionRef, ct).ConfigureAwait(false);
        if (string.IsNullOrEmpty(txHash)) return (null, null);

        var receipt = await _web3!.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash).ConfigureAwait(false);
        var wait = 0;
        while (receipt == null && wait < 30)
        {
            await Task.Delay(1000, ct).ConfigureAwait(false);
            receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash).ConfigureAwait(false);
            wait++;
        }

        if (receipt?.Status?.Value != 1) return (null, txHash);

        var contract = _web3.Eth.GetContract(SupplyChainAbi.Full, _settings.SupplyChainContractAddress);
        var totalFn = contract.GetFunction("getTotalShipments");
        var total = await totalFn.CallAsync<BigInteger>().ConfigureAwait(false);
        if (total <= 0) return (null, txHash);

        var shipmentIdsFn = contract.GetFunction("shipmentIds");
        var idBytes = await shipmentIdsFn.CallAsync<byte[]>(total - 1).ConfigureAwait(false);
        var shipmentIdHex = Bytes32ToHex(idBytes ?? Array.Empty<byte>());
        return (shipmentIdHex, txHash);
    }

    public async Task<int?> GetShipmentStatusFromChainAsync(string shipmentIdHex, CancellationToken ct = default)
    {
        EnsureWeb3();
        if (_web3 == null || string.IsNullOrEmpty(_settings.SupplyChainContractAddress))
            return null;

        var bytes32 = HexToBytes32(shipmentIdHex);
        var contract = _web3.Eth.GetContract(SupplyChainAbi.Full, _settings.SupplyChainContractAddress);
        var fn = contract.GetFunction("getShipmentStatus");
        var status = await fn.CallAsync<byte>(bytes32).ConfigureAwait(false);
        return status;
    }

    public async Task<(string? productId, int? quantity, string? destination, uint? createdAt, int? status)> GetShipmentFromChainAsync(string shipmentIdHex, CancellationToken ct = default)
    {
        var chainStatus = await GetShipmentStatusFromChainAsync(shipmentIdHex, ct).ConfigureAwait(false);
        if (chainStatus == null) return (null, null, null, null, null);
        return (null, null, null, null, chainStatus);
    }

    public async Task<(string? agreementIdHex, string? txHash)> FundPaymentAgreementAsync(string shipmentIdHex, string supplierAddress, decimal amountWei, CancellationToken ct = default)
    {
        EnsureWeb3();
        if (_web3 == null || string.IsNullOrEmpty(_settings.PaymentContractAddress))
            return (null, null);

        var bytes32 = HexToBytes32(shipmentIdHex);
        var wei = new HexBigInteger((BigInteger)amountWei);
        var contract = _web3.Eth.GetContract(PaymentAbi.Full, _settings.PaymentContractAddress);
        var fn = contract.GetFunction("createAndFundAgreement");
        var txHash = await fn.SendTransactionAsync(
            _web3.TransactionManager.Account.Address,
            new HexBigInteger(500_000),
            wei,
            bytes32,
            supplierAddress).ConfigureAwait(false);
        if (string.IsNullOrEmpty(txHash)) return (null, null);

        Nethereum.RPC.Eth.DTOs.TransactionReceipt? receipt = null;
        for (var i = 0; i < 30; i++)
        {
            await Task.Delay(1000, ct).ConfigureAwait(false);
            receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash).ConfigureAwait(false);
            if (receipt != null) break;
        }
        string? agreementIdHex = null;
        if (receipt != null && receipt.Logs != null && receipt.Logs.Count > 0)
        {
            // For this Nethereum version, logs are JTokens; extract topics[1] from the first log
            if (receipt.Logs[0] is JToken log)
            {
                var topics = log["topics"] as JArray;
                if (topics != null && topics.Count > 1)
                {
                    agreementIdHex = topics[1]?.ToString();
                }
            }
        }
        return (agreementIdHex, txHash);
    }

    public async Task<string?> ConfirmDeliveryAndReleasePaymentAsync(string agreementIdHex, CancellationToken ct = default)
    {
        EnsureWeb3();
        if (_web3 == null || string.IsNullOrEmpty(_settings.PaymentContractAddress))
            return null;

        var bytes32 = HexToBytes32(agreementIdHex);
        var contract = _web3.Eth.GetContract(PaymentAbi.Full, _settings.PaymentContractAddress);
        var fn = contract.GetFunction("confirmDeliveryAndRelease");
        var txHash = await fn.SendTransactionAsync(
            _web3.TransactionManager.Account.Address,
            new HexBigInteger(500_000),
            null,
            bytes32).ConfigureAwait(false);
        return txHash;
    }

    public async Task<int?> GetPaymentAgreementStatusAsync(string agreementIdHex, CancellationToken ct = default)
    {
        EnsureWeb3();
        if (_web3 == null || string.IsNullOrEmpty(_settings.PaymentContractAddress))
            return null;

        var bytes32 = HexToBytes32(agreementIdHex);
        var contract = _web3.Eth.GetContract(PaymentAbi.Full, _settings.PaymentContractAddress);
        var fn = contract.GetFunction("getAgreementStatus");
        var status = await fn.CallAsync<byte>(bytes32).ConfigureAwait(false);
        return status;
    }

    public static string GetStatusName(int status)
    {
        return status >= 0 && status < StatusNames.Length ? StatusNames[status] : "Unknown";
    }

    public static string GetPaymentStatusName(int status)
    {
        return status >= 0 && status < PaymentStatusNames.Length ? PaymentStatusNames[status] : "Unknown";
    }
}
