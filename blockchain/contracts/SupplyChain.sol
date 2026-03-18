// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

/**
 * @title SupplyChain
 * @dev Enterprise-grade supply chain contract for immutable shipment recording and status updates.
 * Roles: Supplier, Manufacturer, Transporter, Distributor, Retailer.
 */
contract SupplyChain {
    enum ShipmentStatus {
        Created,
        Dispatched,
        InTransit,
        Delivered
    }

    struct Shipment {
        bytes32 shipmentId;
        string productId;
        uint256 quantity;
        string destination;
        uint256 createdAt;
        ShipmentStatus status;
        address createdBy;
        string transactionRef;
    }

    mapping(bytes32 => Shipment) public shipments;
    bytes32[] public shipmentIds;
    mapping(bytes32 => uint256) public shipmentIndex;

    address public owner;
    mapping(address => bool) public authorizedRoles;

    event ShipmentCreated(
        bytes32 indexed shipmentId,
        string productId,
        uint256 quantity,
        string destination,
        address createdBy,
        string transactionRef
    );

    event ShipmentStatusUpdated(
        bytes32 indexed shipmentId,
        ShipmentStatus previousStatus,
        ShipmentStatus newStatus,
        address updatedBy
    );

    modifier onlyOwner() {
        require(msg.sender == owner, "Not contract owner");
        _;
    }

    modifier onlyAuthorized() {
        require(authorizedRoles[msg.sender] || msg.sender == owner, "Not authorized");
        _;
    }

    constructor() {
        owner = msg.sender;
        authorizedRoles[msg.sender] = true;
    }

    function addAuthorized(address account) external onlyOwner {
        authorizedRoles[account] = true;
    }

    function removeAuthorized(address account) external onlyOwner {
        authorizedRoles[account] = false;
    }

    /**
     * @dev Create a new shipment record. Emits ShipmentCreated.
     */
    function createShipment(
        string calldata productId,
        uint256 quantity,
        string calldata destination,
        string calldata transactionRef
    ) external onlyAuthorized returns (bytes32) {
        bytes32 id = keccak256(
            abi.encodePacked(
                productId,
                block.timestamp,
                msg.sender,
                block.prevrandao
            )
        );
        require(shipments[id].createdAt == 0, "Shipment ID collision");

        shipments[id] = Shipment({
            shipmentId: id,
            productId: productId,
            quantity: quantity,
            destination: destination,
            createdAt: block.timestamp,
            status: ShipmentStatus.Created,
            createdBy: msg.sender,
            transactionRef: transactionRef
        });

        shipmentIds.push(id);
        shipmentIndex[id] = shipmentIds.length - 1;

        emit ShipmentCreated(id, productId, quantity, destination, msg.sender, transactionRef);
        return id;
    }

    /**
     * @dev Update shipment status. Only Dispatched -> InTransit -> Delivered progression allowed.
     */
    function updateShipmentStatus(bytes32 shipmentId, ShipmentStatus newStatus) external onlyAuthorized {
        Shipment storage s = shipments[shipmentId];
        require(s.createdAt != 0, "Shipment does not exist");

        ShipmentStatus prev = s.status;
        require(uint256(newStatus) > uint256(prev) && uint256(newStatus) <= uint256(ShipmentStatus.Delivered), "Invalid status transition");

        s.status = newStatus;
        emit ShipmentStatusUpdated(shipmentId, prev, newStatus, msg.sender);
    }

    function getShipment(bytes32 shipmentId) external view returns (
        bytes32 id,
        string memory productId,
        uint256 quantity,
        string memory destination,
        uint256 createdAt,
        ShipmentStatus status,
        address createdBy,
        string memory transactionRef
    ) {
        Shipment memory s = shipments[shipmentId];
        require(s.createdAt != 0, "Shipment does not exist");
        return (
            s.shipmentId,
            s.productId,
            s.quantity,
            s.destination,
            s.createdAt,
            s.status,
            s.createdBy,
            s.transactionRef
        );
    }

    function getShipmentStatus(bytes32 shipmentId) external view returns (ShipmentStatus) {
        return shipments[shipmentId].status;
    }

    function getTotalShipments() external view returns (uint256) {
        return shipmentIds.length;
    }
}
