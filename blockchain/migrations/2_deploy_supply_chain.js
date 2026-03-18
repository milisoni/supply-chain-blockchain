const SupplyChain = artifacts.require("SupplyChain");
const SupplyChainPayment = artifacts.require("SupplyChainPayment");

// Backend signer address (derived from Blockchain:AccountPrivateKey in appsettings.Development.json).
// Update this whenever the backend private key changes.
const BACKEND_SIGNER = "0xfC5775Ca6Fd4e765D55Bb78675b503795d30A009";

module.exports = async function (deployer, network, accounts) {
  await deployer.deploy(SupplyChain);
  const supplyChain = await SupplyChain.deployed();

  await deployer.deploy(SupplyChainPayment, supplyChain.address);
  const payment = await SupplyChainPayment.deployed();

  // Authorize the payment contract to call SupplyChain (e.g. future status checks)
  await supplyChain.addAuthorized(payment.address);

  // Authorize the backend signer on both contracts so it can send transactions
  if (BACKEND_SIGNER && BACKEND_SIGNER !== accounts[0]) {
    await supplyChain.addAuthorized(BACKEND_SIGNER);
    await payment.addAuthorized(BACKEND_SIGNER);
    console.log("Authorized backend signer:", BACKEND_SIGNER);
  }

  console.log("SupplyChain:       ", supplyChain.address);
  console.log("SupplyChainPayment:", payment.address);
};
