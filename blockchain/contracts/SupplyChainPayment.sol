// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

/**
 * @title SupplyChainPayment
 * @dev Escrow-style payment: buyer deposits funds; release to supplier only after delivery confirmation.
 * Integrated with SupplyChain contract for delivery verification.
 */
interface ISupplyChain {
    function getShipmentStatus(bytes32 shipmentId) external view returns (uint8);
}

contract SupplyChainPayment {
    enum PaymentStatus {
        Pending,
        Funded,
        Delivered,
        Released,
        Refunded
    }

    struct PaymentAgreement {
        bytes32 agreementId;
        bytes32 shipmentId;
        address buyer;
        address supplier;
        uint256 amountWei;
        uint256 createdAt;
        PaymentStatus status;
    }

    mapping(bytes32 => PaymentAgreement) public agreements;
    bytes32[] public agreementIds;

    address public owner;
    ISupplyChain public supplyChain;
    mapping(address => bool) public authorized;

    event PaymentFunded(bytes32 indexed agreementId, bytes32 shipmentId, address buyer, uint256 amount);
    event PaymentReleased(bytes32 indexed agreementId, address supplier, uint256 amount);
    event DeliveryConfirmed(bytes32 indexed agreementId, bytes32 shipmentId);

    modifier onlyOwner() {
        require(msg.sender == owner, "Not owner");
        _;
    }

    modifier onlyAuthorized() {
        require(authorized[msg.sender] || msg.sender == owner, "Not authorized");
        _;
    }

    constructor(address _supplyChainAddress) {
        owner = msg.sender;
        authorized[msg.sender] = true;
        supplyChain = ISupplyChain(_supplyChainAddress);
    }

    function setSupplyChain(address _supplyChainAddress) external onlyOwner {
        supplyChain = ISupplyChain(_supplyChainAddress);
    }

    function addAuthorized(address account) external onlyOwner {
        authorized[account] = true;
    }

    /**
     * @dev Create agreement and fund escrow. Buyer sends ETH with this call.
     */
    function createAndFundAgreement(
        bytes32 shipmentId,
        address supplier
    ) external payable onlyAuthorized returns (bytes32) {
        require(msg.value > 0, "Amount must be > 0");
        require(supplier != address(0), "Invalid supplier");

        bytes32 agreementId = keccak256(
            abi.encodePacked(shipmentId, msg.sender, block.timestamp, block.prevrandao)
        );
        require(agreements[agreementId].createdAt == 0, "Agreement exists");

        agreements[agreementId] = PaymentAgreement({
            agreementId: agreementId,
            shipmentId: shipmentId,
            buyer: msg.sender,
            supplier: supplier,
            amountWei: msg.value,
            createdAt: block.timestamp,
            status: PaymentStatus.Funded
        });
        agreementIds.push(agreementId);

        emit PaymentFunded(agreementId, shipmentId, msg.sender, msg.value);
        return agreementId;
    }

    /**
     * @dev Confirm delivery and release payment to supplier.
     * Only when shipment status is Delivered (3) in SupplyChain contract.
     */
    function confirmDeliveryAndRelease(bytes32 agreementId) external onlyAuthorized {
        PaymentAgreement storage ag = agreements[agreementId];
        require(ag.createdAt != 0, "Agreement not found");
        require(ag.status == PaymentStatus.Funded, "Invalid status");

        uint8 status = supplyChain.getShipmentStatus(ag.shipmentId);
        require(status == 3, "Shipment not delivered"); // 3 = Delivered enum value

        ag.status = PaymentStatus.Released;
        emit DeliveryConfirmed(agreementId, ag.shipmentId);
        emit PaymentReleased(agreementId, ag.supplier, ag.amountWei);

        (bool sent, ) = ag.supplier.call{value: ag.amountWei}("");
        require(sent, "Transfer failed");
    }

    function getAgreement(bytes32 agreementId) external view returns (
        bytes32 id,
        bytes32 shipmentId,
        address buyer,
        address supplier,
        uint256 amountWei,
        uint256 createdAt,
        PaymentStatus status
    ) {
        PaymentAgreement memory ag = agreements[agreementId];
        require(ag.createdAt != 0, "Agreement not found");
        return (
            ag.agreementId,
            ag.shipmentId,
            ag.buyer,
            ag.supplier,
            ag.amountWei,
            ag.createdAt,
            ag.status
        );
    }

    function getAgreementStatus(bytes32 agreementId) external view returns (PaymentStatus) {
        return agreements[agreementId].status;
    }
}
