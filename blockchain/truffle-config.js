const path = require("path");

module.exports = {
  contracts_build_directory: path.join(__dirname, "build", "contracts"),
  contracts_directory: path.join(__dirname, "contracts"),
  migrations_directory: path.join(__dirname, "migrations"),
  networks: {
    development: {
      host: "127.0.0.1",
      port: 7545,
      network_id: "*",
    },
    ganache: {
      host: "127.0.0.1",
      port: 7545,
      network_id: "5777",
    },
  },
  compilers: {
    solc: {
      version: "0.8.19",
      settings: {
        optimizer: {
          enabled: true,
          runs: 200,
        },
      },
    },
  },
};
