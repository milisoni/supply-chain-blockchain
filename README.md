# Blockchain-Based Supply Chain Management & Automated Payment System

Enterprise-grade supply chain and payment solution using **ASP.NET Core Web API**, **Angular**, **MongoDB**, and **Ethereum** (Solidity smart contracts). Includes JWT role-based authentication, BCrypt password hashing, immutable shipment recording, status updates, and escrow payment release on delivery confirmation.

**🚀 New to this project?** → [Complete Setup Guide](SETUP_GUIDE.md) — Step-by-step instructions for cloning, installing, and running locally.

---

## 🔐 Important: Before Pushing to GitHub

### Secrets Already Protected
This repo includes a `.gitignore` file that **automatically excludes**:
- ✅ `appsettings.Development.json` (contains private keys & contract addresses)
- ✅ `node_modules/` and build outputs (`/bin`, `/obj`, `/dist`)
- ✅ Any `.env` files or `mnemonic.txt`

### Push to GitHub (First Time)

```powershell
# From repository root
cd "E:\Major Project"

# Initialize and push
git init
git add .
git commit -m "Initial commit: Blockchain supply chain management system"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/supply-chain-blockchain.git
git push -u origin main
```

Replace `YOUR_USERNAME` with your actual GitHub username. The `.gitignore` will prevent secrets from being committed.

### Verify Before Push
```powershell
git status
# Should NOT show:
# - appsettings.Development.json
# - node_modules/
# - /bin, /obj, /dist folders
```

---

## 📋 First-Time Setup for Others (Cloning)

Anyone cloning this repo will need to:
1. Install prerequisites (Node.js, .NET 8, MongoDB, Ganache)
2. Create their own `appsettings.Development.json` with their Ganache account details
3. Deploy contracts to their local Ganache
4. Update backend config with their contract addresses

**Complete guide:** [SETUP_GUIDE.md](SETUP_GUIDE.md)

---

## Architecture Overview

| Layer | Technology | Purpose |
|-------|------------|---------|
| **Frontend** | Angular 18, Bootstrap 5, TypeScript | User interface: login, dashboard, shipments, track, payments, transaction history, admin |
| **Backend** | ASP.NET Core 8 Web API | REST APIs, JWT auth, BCrypt, MongoDB, Nethereum blockchain integration |
| **Database** | MongoDB | Off-chain data: users, shipment records, payment agreements, transaction history |
| **Blockchain** | Ethereum (Ganache), Solidity | Immutable shipment records and escrow payment (release on delivery) |

---

## Repository Structure

```
Major Project/
├── blockchain/                 # Smart contracts & Truffle
│   ├── contracts/
│   │   ├── SupplyChain.sol     # Shipment create/update, status enum
│   │   ├── SupplyChainPayment.sol  # Escrow: fund → release on delivery
│   │   └── Migrations.sol
│   ├── migrations/
│   ├── truffle-config.js
│   └── package.json
├── backend/                    # ASP.NET Core API
│   ├── SupplyChainApi/
│   │   ├── Controllers/        # Auth, Shipments, Payments, TransactionHistory, Admin, Users
│   │   ├── Services/           # AuthService, BlockchainService (Nethereum)
│   │   ├── Data/               # MongoDbContext
│   │   ├── Models/             # User, ShipmentRecord, TransactionHistory, PaymentAgreementRecord
│   │   ├── DTOs/
│   │   ├── Configuration/
│   │   └── Abi/                # SupplyChain & Payment contract ABIs
│   └── SupplyChain.sln
├── frontend/                   # Angular 18 SPA
│   └── src/app/
│       ├── layout/             # Navbar + router-outlet
│       ├── pages/              # Login, Register, Dashboard, Shipments, Track, Status, Payments, Transactions, Profile, Admin
│       ├── services/           # AuthService, ApiService
│       ├── guards/             # authGuard, adminGuard
│       └── interceptors/       # jwtInterceptor
└── README.md
```

---

## Implementations in Detail

### 1. User Registration and Login (Use Case 1)

- **Backend**
  - `AuthController`: `POST /api/auth/register`, `POST /api/auth/login`.
  - `AuthService`: Registers user with **BCrypt** (work factor 12) hashed password; roles: Admin, Supplier, Manufacturer, Transporter, Distributor, Retailer. Login verifies password and issues **JWT** (configurable expiry, signing key in `appsettings.json`).
  - JWT contains claims: `NameIdentifier` (userId), `Email`, `Name`, `Role`. Used for authorization on all protected endpoints.
- **Frontend**
  - Login and Register pages; on success, token and user stored in `localStorage` and `AuthService` signals; redirect to dashboard.
  - `jwtInterceptor` attaches `Authorization: Bearer <token>` to every API request.

### 2. Add Shipment Details (Use Case 2)

- **Blockchain**
  - `SupplyChain.sol`: `createShipment(productId, quantity, destination, transactionRef)` — only callable by `owner` or `authorizedRoles`. Emits `ShipmentCreated` and returns `bytes32 shipmentId`.
- **Backend**
  - `ShipmentsController.Create`: Validates input, calls `IBlockchainService.GetCreateShipmentResultAsync` (sends transaction via Nethereum, waits for receipt, reads new `shipmentIds[index]` to get `shipmentId` hex). Saves off-chain `ShipmentRecord` in MongoDB and a `TransactionHistory` entry with transaction hash.
- **Frontend**
  - “Add Shipment” form: product ID, quantity, destination, optional transaction ref; submit calls API and redirects to shipments list.

### 3. Update Shipment Status (Use Case 3)

- **Blockchain**
  - `SupplyChain.sol`: `updateShipmentStatus(shipmentId, newStatus)` with allowed progression: Created → Dispatched → InTransit → Delivered.
- **Backend**
  - `ShipmentsController.UpdateStatus`: Sends status update to chain via Nethereum, then updates MongoDB `ShipmentRecord` and logs to `TransactionHistory`.
- **Frontend**
  - “Update Status” page: enter blockchain shipment ID, select status (Dispatched / In Transit / Delivered), submit.

### 4. Track Shipment (Use Case 4)

- **Backend**
  - `ShipmentsController.Track(blockchainShipmentId)`: Reads off-chain record from MongoDB and optionally current status from chain via `GetShipmentStatusFromChainAsync`; returns product, quantity, destination, status, transaction hash.
- **Frontend**
  - “Track” page: input shipment ID (blockchain hex), display current status and details.

### 5. Automated Payment Execution (Use Case 5)

- **Blockchain**
  - `SupplyChainPayment.sol`:
    - `createAndFundAgreement(shipmentId, supplier)` **payable**: Stores agreement (buyer = msg.sender, amount = msg.value), emits `PaymentFunded`, returns `agreementId`.
    - `confirmDeliveryAndRelease(agreementId)`: Checks `supplyChain.getShipmentStatus(shipmentId) == Delivered (3)`; then transfers `amountWei` to `supplier` and sets status to Released.
  - Deployment: Payment contract is deployed with `SupplyChain` address; both can be authorized to call each other if needed.
- **Backend**
  - `PaymentsController.FundAgreement`: Validates supplier user has `BlockchainAddress`, calls `BlockchainService.FundPaymentAgreementAsync` (sends ETH via Nethereum), parses receipt logs for `PaymentFunded` to get `agreementId`, stores `PaymentAgreementRecord` in MongoDB.
  - `PaymentsController.ReleasePayment`: Calls `ConfirmDeliveryAndReleasePaymentAsync(agreementId)`; on success updates MongoDB and logs transaction.
- **Frontend**
  - Payments page: fund (shipment ID, supplier user ID, amount in Wei); release (agreement ID). Lists user’s payment agreements.

### 6. Transaction History & Admin

- **Backend**
  - `TransactionHistoryController`: Returns transactions (Admin: all; others: own). Stored in MongoDB when any blockchain tx is submitted.
  - `AdminController`: Role `Admin` only. Endpoints: list/update users, overview (counts of users, shipments, transactions, payments).
- **Frontend**
  - Transactions page: table of type, hash, details, date.
  - Admin overview: cards with counts and links to users/transactions.
  - Admin users: table with edit (name, blockchain address, isActive).

### 7. Security and Configuration

- **Authentication**: JWT Bearer; key, issuer, audience, expiry in `appsettings.json` (use strong key in production).
- **Passwords**: BCrypt only (no plain text).
- **Blockchain**: `appsettings.json` → `Blockchain:RpcUrl`, `SupplyChainContractAddress`, `PaymentContractAddress`, `AccountPrivateKey` (wallet that pays gas and sends txs). Empty addresses/keys disable chain calls gracefully.

---

## How to Run

### Prerequisites

- Node.js 16+, npm
- .NET 8 SDK
- MongoDB (local or Atlas; set connection string in `appsettings.json`)
- Ganache (local Ethereum chain on port 7545)

### 1. Blockchain (Ganache + Truffle)

```bash
cd blockchain
npm install
# Start Ganache (GUI or CLI) on port 7545
npx truffle migrate --network development
# Note the deployed contract addresses and set them (and an account private key) in backend appsettings.
```

### 2. Backend API

- Set `MongoDb:ConnectionString` and `Blockchain:*` in `appsettings.Development.json`. Use the **same Ethereum account that deployed the contracts** (e.g. first Ganache account) as `AccountPrivateKey` and set `SupplyChainContractAddress` and `PaymentContractAddress` from the Truffle migration output.
- From `backend`: `dotnet run --project SupplyChainApi`.
- API: http://localhost:5000; Swagger: http://localhost:5000/swagger.

### 3. Frontend

```bash
cd frontend
npm install
npm start
```

- App: http://localhost:4200. Use Register then Login; create shipments, track, update status, fund/release payments; use Admin account for overview and user management.

### 4. CORS

- API allows `http://localhost:4200` by default. Adjust in `Program.cs` if needed.

---

## API Summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/register | Register (name, email, password, role) |
| POST | /api/auth/login | Login (email, password) → JWT |
| GET | /api/shipments | List shipments (myOnly query) |
| POST | /api/shipments | Create shipment (blockchain + MongoDB) |
| PUT | /api/shipments/status | Update status (blockchain + MongoDB) |
| GET | /api/shipments/track/{id} | Track by blockchain ID |
| POST | /api/payments/fund | Fund escrow (shipmentId, supplierUserId, amountWei) |
| POST | /api/payments/release | Release payment (blockchainAgreementId) |
| GET | /api/transactionhistory | Transaction history (role-filtered) |
| GET | /api/users/me | Current user profile |
| PUT | /api/users/me | Update profile (name, blockchainAddress) |
| GET | /api/admin/overview | Admin: counts |
| GET/PUT | /api/admin/users, /api/admin/users/{id} | Admin: list/update users |

All except auth require `Authorization: Bearer <token>`.

---

## Design Notes

- **Off-chain vs on-chain**: User and shipment metadata live in MongoDB; blockchain holds only immutable shipment IDs, status, and payment agreements so the system remains auditable and scalable.
- **Escrow**: Buyer funds the payment contract; release is conditional on shipment status “Delivered” on the SupplyChain contract, implementing automated payment on delivery.
- **Roles**: Enforced in API via `[Authorize(Roles = "Admin")]` and in frontend via `authGuard` / `adminGuard` and `isAdmin` signal.

This delivers a thorough, professional app with the requested use cases, JWT + BCrypt, MongoDB, Ethereum (Ganache/Truffle), and Nethereum integration, plus a clear structure for extension and deployment.
