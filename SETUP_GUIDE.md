# Supply Chain Blockchain Setup Guide

Complete step-by-step guide to clone, install, and run the Supply Chain blockchain project locally.

---

## Part 1: What NOT to Commit to GitHub

**DO NOT add these files to GitHub:**
- `backend/SupplyChainApi/appsettings.Development.json` — contains private keys, RPC URLs, contract addresses.
- Any file with `.pem`, `.key`, or `secret` in the name.
- `mnemonic.txt` from Ganache.
- Ganache workspace files or keystore files.

**Why:** Private keys and contract addresses are environment-specific and expose your test ETH wallets.

**How:** Use `.gitignore` (already included in repo) to block these automatically.

---

## Part 2: Push This Project to GitHub

### 2.1 Create GitHub Repository
1. Go to https://github.com/new
2. Enter repository name (e.g., `supply-chain-blockchain`)
3. Choose Private or Public
4. Do NOT initialize with README (you already have one)
5. Click **Create repository**

### 2.2 Push from Local Machine
```powershell
cd "E:\Major Project"
git init
git add .
git commit -m "Initial commit: supply chain blockchain app"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/supply-chain-blockchain.git
git push -u origin main
```

Replace `YOUR_USERNAME` with your actual GitHub username.

### 2.3 Verify
- Check GitHub repo page — all files should appear except those in `.gitignore`.

---

## Part 3: Setup Instructions for Cloning (For Other Developers)

### 3.1 Prerequisites (Install These)

**Node.js 18.x LTS** (or 20.x LTS)
- Download: https://nodejs.org
- Verify: `node --version` (should be v18.x or v20.x)
- Verify npm: `npm --version` (should be ≥9.x)

**.NET 8 SDK**
- Download: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Verify: `dotnet --version` (should be 8.0.x)

**MongoDB (Local)**
- Download Community: https://www.mongodb.com/try/download/community
- Or use **MongoDB Atlas** (cloud): https://www.mongodb.com/cloud/atlas (create free account, cluster)
- Verify: `mongod --version` or test connection in Atlas

**Ganache CLI or GUI**
- GUI: https://trufflesuite.com/ganache
- CLI: `npm install -g ganache-cli` then `ganache-cli --deterministic`

**Git**
- Download: https://git-scm.com/download/win
- Verify: `git --version`

---

### 3.2 Clone the Repository
```powershell
git clone https://github.com/YOUR_USERNAME/supply-chain-blockchain.git
cd supply-chain-blockchain
```

---

### 3.3 Set Up Backend

#### 3.3.1 Create `appsettings.Development.json`
Create the file at `backend/SupplyChainApi/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "SupplyChainDb"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyForJwtSigning_Min32Chars!",
    "Issuer": "SupplyChainApi",
    "Audience": "SupplyChainApp",
    "ExpiryMinutes": 60
  },
  "Blockchain": {
    "RpcUrl": "http://127.0.0.1:7545",
    "SupplyChainContractAddress": "",
    "PaymentContractAddress": "",
    "AccountPrivateKey": ""
  }
}
```

**Leave Blockchain fields empty for now** — will fill after contract deployment.

#### 3.3.2 MongoDB Setup

**Option A: Local MongoDB**
```powershell
mongod
# In another terminal, verify:
mongo --eval "db.adminCommand('ping')"
```

**Option B: MongoDB Atlas (Cloud)**
1. Create free account at https://www.mongodb.com/cloud/atlas
2. Create cluster (free tier)
3. Get connection string: `mongodb+srv://username:password@cluster.mongodb.net/`
4. Add username/password in connection string
5. Replace `ConnectionString` in `appsettings.Development.json`

#### 3.3.3 Restore Backend Dependencies
```powershell
cd backend/SupplyChainApi
dotnet restore
```

---

### 3.4 Set Up Blockchain (Smart Contracts)

#### 3.4.1 Start Ganache
**Using Ganache GUI:**
1. Open Ganache
2. Click **New Workspace**
3. Leave defaults (port 7545, network ID 5777)
4. Start

**Using Ganache CLI:**
```powershell
ganache-cli --deterministic --host 127.0.0.1 --port 7545
```

#### 3.4.2 Copy Ganache Account Keys
1. In Ganache (GUI or running CLI), copy **Account 1** private key (starts with `0x`)
2. Copy **any other account public address** (for testing)

#### 3.4.3 Deploy Contracts
```powershell
cd blockchain
npm install
npx truffle migrate --network development
```

**Output Example:**
```
SupplyChain deployed to: 0xd0aF1FA925637e389C52Bd9ebaCca9e4AE1F86E7
SupplyChainPayment deployed to: 0x7b7FF328B91BB794131cc439f8e167b2dEe25cb7
```

#### 3.4.4 Update Backend Config with Contract Addresses
Edit `backend/SupplyChainApi/appsettings.Development.json`:

```json
"Blockchain": {
  "RpcUrl": "http://127.0.0.1:7545",
  "SupplyChainContractAddress": "0xd0aF1FA925637e389C52Bd9ebaCca9e4AE1F86E7",
  "PaymentContractAddress": "0x7b7FF328B91BB794131cc439f8e167b2dEe25cb7",
  "AccountPrivateKey": "0x<paste_account_1_private_key_here>"
}
```

---

### 3.5 Start Backend API
```powershell
cd backend/SupplyChainApi
dotnet run
```

**Expected:**
- API running at `http://localhost:5000`
- Swagger UI at `http://localhost:5000/swagger`

---

### 3.6 Start Frontend

#### 3.6.1 Install Dependencies
```powershell
cd frontend
npm install
```

#### 3.6.2 Start Dev Server
```powershell
npm start
```

**Expected:**
- Frontend at `http://localhost:4200`
- Auto-opens in browser

---

### 3.7 Full System Check

1. **Ganache:** Running on port 7545, Accounts visible
2. **MongoDB:** Connected (Local or Atlas)
3. **Backend:** `http://localhost:5000/swagger` accessible
4. **Frontend:** `http://localhost:4200` loads
5. **Smart Contracts:** Deployed on Ganache with provided addresses in backend config

---

## Part 4: Using the App (First Time Setup)

### 4.1 Register Users
1. Open `http://localhost:4200`
2. Click **Register**
3. Create Account 1:
   - Name: `Supplier`
   - Email: `supplier@example.com`
   - Password: `password123`
   - Role: `Supplier`
4. Login
5. Go to **Profile** → set Blockchain Address to **Account 2** public address from Ganache
6. Logout

### 4.2 Register Account 2 (Buyer)
1. Register:
   - Name: `Buyer`
   - Email: `buyer@example.com`
   - Password: `password123`
   - Role: `Manufacturer`
2. Login (buyer account)
3. Go to **Profile** → set Blockchain Address to **Account 3** from Ganache
4. Logout

### 4.3 Test Full Flow (As Buyer)
1. Login as buyer (`buyer@example.com`)
2. **Add Shipment:**
   - Product ID: `PROD-001`
   - Quantity: `100`
   - Destination: `New York`
   - Submit → copy returned Blockchain Shipment ID
3. **Update Status to Delivered:**
   - Go to **Update Status**
   - Paste Shipment ID → select **Delivered** → Submit
4. **Fund Payment:**
   - Go to **Payments**
   - Blockchain Shipment ID: `<shipment_id>`
   - Supplier User ID: (get from Admin → Users, copy Supplier user's `id`)
   - Amount: `1000000000000000000` (1 ETH in Wei)
   - Click **Fund** → copy Agreement ID
5. **Release Payment:**
   - Paste Agreement ID in Release section
   - Click **Confirm delivery & release**
6. **Verify:**
   - Go to **Transactions** — should see `FundPayment` and `ReleasePayment`
   - In Ganache, Account 2 (Supplier) balance should increase

---

## Part 5: Troubleshooting

| Error | Solution |
|-------|----------|
| `Connection refused` to API | Start backend: `dotnet run` |
| `Insufficient funds` | Use Account 1 private key (pre-funded) in appsettings |
| `Not authorized` | Ensure Account 1 is authorized on contracts (redeploy or run Truffle console commands) |
| `Invalid status transition` | Use Delivered status only after Dispatched and InTransit are set |
| `Cannot find module` (Node) | `npm install` in both `blockchain` and `frontend` folders |
| `MongoDB connection error` | Verify MongoDB is running locally or Atlas connection string is correct |

---

## Part 6: Environment-Specific Setup

### Development (Your Local Machine)
- Use local Ganache (port 7545)
- Use local MongoDB
- `.gitignore` auto-protects secrets

### Staging/Production (Future)
- Use testnet (e.g., Sepolia): update RPC URL
- Use managed MongoDB (Atlas)
- Store sensitive config in environment variables, not files
- Never commit private keys anywhere

---

## Quick Reference: File Locations

| What | Where |
|------|-------|
| Backend config | `backend/SupplyChainApi/appsettings.Development.json` |
| Contracts | `blockchain/contracts/*.sol` |
| Frontend | `frontend/src/app` |
| Smart contract ABIs | `backend/SupplyChainApi/Abi/*.cs` |
| MongoDB models | `backend/SupplyChainApi/Models/*.cs` |
| API routes | `backend/SupplyChainApi/Controllers/*.cs` |

---

## Support

For issues or questions:
1. Check backend logs: `dotnet run` terminal
2. Check frontend logs: browser console (F12)
3. Check Ganache transactions tab
4. Verify all services (Ganache, MongoDB, Backend, Frontend) are running

