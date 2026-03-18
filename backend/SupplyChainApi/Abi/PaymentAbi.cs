namespace SupplyChainApi.Abi;

public static class PaymentAbi
{
    public const string Full = """
[
  {
    "inputs": [
      { "internalType": "bytes32", "name": "shipmentId", "type": "bytes32" },
      { "internalType": "address", "name": "supplier", "type": "address" }
    ],
    "name": "createAndFundAgreement",
    "outputs": [
      { "internalType": "bytes32", "name": "", "type": "bytes32" }
    ],
    "stateMutability": "payable",
    "type": "function"
  },
  {
    "inputs": [
      { "internalType": "bytes32", "name": "agreementId", "type": "bytes32" }
    ],
    "name": "confirmDeliveryAndRelease",
    "outputs": [],
    "stateMutability": "nonpayable",
    "type": "function"
  },
  {
    "inputs": [
      { "internalType": "bytes32", "name": "agreementId", "type": "bytes32" }
    ],
    "name": "getAgreementStatus",
    "outputs": [
      { "internalType": "uint8", "name": "", "type": "uint8" }
    ],
    "stateMutability": "view",
    "type": "function"
  }
]
""";
}
