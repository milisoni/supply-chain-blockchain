namespace SupplyChainApi.Abi;

public static class SupplyChainAbi
{
    /// <summary>Minimal ABI for supply chain contract calls (create, update, get).</summary>
        public const string Full = """
[
    {
        "inputs": [
            { "internalType": "string", "name": "productId", "type": "string" },
            { "internalType": "uint256", "name": "quantity", "type": "uint256" },
            { "internalType": "string", "name": "destination", "type": "string" },
            { "internalType": "string", "name": "transactionRef", "type": "string" }
        ],
        "name": "createShipment",
        "outputs": [
            { "internalType": "bytes32", "name": "", "type": "bytes32" }
        ],
        "stateMutability": "nonpayable",
        "type": "function"
    },
    {
        "inputs": [
            { "internalType": "bytes32", "name": "shipmentId", "type": "bytes32" },
            { "internalType": "uint8", "name": "newStatus", "type": "uint8" }
        ],
        "name": "updateShipmentStatus",
        "outputs": [],
        "stateMutability": "nonpayable",
        "type": "function"
    },
    {
        "inputs": [
            { "internalType": "bytes32", "name": "shipmentId", "type": "bytes32" }
        ],
        "name": "getShipment",
        "outputs": [
            { "internalType": "bytes32", "name": "id", "type": "bytes32" },
            { "internalType": "string", "name": "productId", "type": "string" },
            { "internalType": "uint256", "name": "quantity", "type": "uint256" },
            { "internalType": "string", "name": "destination", "type": "string" },
            { "internalType": "uint256", "name": "createdAt", "type": "uint256" },
            { "internalType": "uint8", "name": "status", "type": "uint8" },
            { "internalType": "address", "name": "createdBy", "type": "address" },
            { "internalType": "string", "name": "transactionRef", "type": "string" }
        ],
        "stateMutability": "view",
        "type": "function"
    },
    {
        "inputs": [
            { "internalType": "bytes32", "name": "shipmentId", "type": "bytes32" }
        ],
        "name": "getShipmentStatus",
        "outputs": [
            { "internalType": "uint8", "name": "", "type": "uint8" }
        ],
        "stateMutability": "view",
        "type": "function"
    },
    {
        "inputs": [],
        "name": "getTotalShipments",
        "outputs": [
            { "internalType": "uint256", "name": "", "type": "uint256" }
        ],
        "stateMutability": "view",
        "type": "function"
    },
    {
        "inputs": [
            { "internalType": "uint256", "name": "", "type": "uint256" }
        ],
        "name": "shipmentIds",
        "outputs": [
            { "internalType": "bytes32", "name": "", "type": "bytes32" }
        ],
        "stateMutability": "view",
        "type": "function"
    }
]
""";
}
