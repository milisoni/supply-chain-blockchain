namespace SupplyChainApi.Configuration;

public class BlockchainSettings
{
    public const string SectionName = "Blockchain";
    public string RpcUrl { get; set; } = string.Empty;
    public string SupplyChainContractAddress { get; set; } = string.Empty;
    public string PaymentContractAddress { get; set; } = string.Empty;
    public string AccountPrivateKey { get; set; } = string.Empty;
}
