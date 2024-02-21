import { ethers } from "hardhat";
import { LedgerSigner } from "./ledger_signer";

const rpcUrl = process.env.RPC_URL;
const provider = new ethers.providers.JsonRpcProvider(rpcUrl);
const index = process.env.NONCE_RESERVED_DEPLOYER_INDEX;
const operatorAllowlist = process.env.OPERATOR_ALLOWLIST;
const nonceReservedDeployerSecret = process.env.NONCE_RESERVED_DEPLOYER_SECRET;
const gasOverrides = {
  // use parameter to set tip for EIP1559 transaction (gas fee)
  maxPriorityFeePerGas: 10e9, // 10 Gwei. This must exceed minimum gas fee expectation from the chain
  maxFeePerGas: 15e9, // 15 Gwei
};
const contractAddress = process.env.GRANT_MINTER_ROLE_CONTRACT_ADDRESS;
const minterAddress = process.env.GRANT_MINTER_ROLE_WALLET_ADDRESS;

async function grantMinterRole() {
  if (operatorAllowlist === undefined) {
    throw new Error("Please set your OPERATOR_ALLOWLIST in a .env file");
  }
  
  // set up deployer
  let deployer;
  if (nonceReservedDeployerSecret == "ledger") {
      try {
          const derivationPath = `m/44'/60'/${parseInt(index)}'/0/0`;
          console.log(`Getting LedgerSigner with path: ${derivationPath}`);
          deployer = new LedgerSigner(provider, derivationPath);
          console.log(`Created LedgerSigner`);
      } catch (err) {
          console.log(`Failed to create LedgerSigner: ${err}`);
      }
  } else {
      console.log(`Creating Private Key Signer...`);
      deployer = new ethers.Wallet(nonceReservedDeployerSecret, provider);
  }

  // get deployer address
  console.log("Waiting to get deployer's address");
  const deployerAddress = await deployer.getAddress();
  console.log("Reserved deployer address is: ", deployerAddress);

  // check account balance
  console.log(
    "Account balance:",
    ethers.utils.formatEther(await deployer.getBalance())
  );

  // Grant minter role
  try {
    const abi = ['function grantMinterRole(address user) public'];
    const tokenContract = new ethers.Contract(contractAddress, abi, deployer);
    const grantTransaction = await tokenContract.grantMinterRole(minterAddress, gasOverrides);
    await grantTransaction.wait();
    console.log("Granted minter role to ", minterAddress);
  } catch (error) {
    console.log("Failed to grant minter role: ", error);
  }
}

grantMinterRole().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
