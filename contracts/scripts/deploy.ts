import { ethers } from "hardhat";
import { RunnerCharacter, RunnerCharacter__factory, RunnerSkin, RunnerSkin__factory, RunnerToken, RunnerToken__factory } from "../typechain-types";
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

async function deploy() {
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

  // deploy ERC721 contracts
  const characterFactory: RunnerCharacter__factory = await ethers.getContractFactory(
    "RunnerCharacter"
  );
  const runnerCharacterName = "Immutable Runner Character"
  const characterContract: RunnerCharacter = await characterFactory.connect(deployer).deploy(
    deployerAddress, // owner
    runnerCharacterName, // name
    "IMRC", // symbol
    "https://json-server-sgiz.onrender.com/character/", // baseURI
    "https://json-server-sgiz.onrender.com/character/", // contractURI
    operatorAllowlist, // operator allowlist
    deployerAddress, // royalty recipient
    ethers.BigNumber.from("2000"), // fee numerator
    gasOverrides, // tipOverrides
  );
  await characterContract.deployed();
  console.log(`${runnerCharacterName} contract deployed to ${characterContract.address}`);

  const skinFactory: RunnerSkin__factory = await ethers.getContractFactory(
    "RunnerSkin"
  );
  const runnerSkinName = "Immutable Runner Skin"
  const skinContract: RunnerSkin = await skinFactory.connect(deployer).deploy(
    deployerAddress, // owner
    runnerSkinName, // name
    "IMRS", // symbol
    "https://json-server-sgiz.onrender.com/skin/", // baseURI
    "https://json-server-sgiz.onrender.com/skin/", // contractURI
    operatorAllowlist, // operator allowlist
    deployerAddress, // royalty recipient
    ethers.BigNumber.from("2000"), // fee numerator
    gasOverrides, // tipOverrides
  );
  await skinContract.deployed();
  console.log(`${runnerSkinName} contract deployed to ${skinContract.address}`);

  const tokenFactory: RunnerToken__factory = await ethers.getContractFactory(
    "RunnerToken"
  );
  const runnerTokenName = "Immutable Runner Token"
  const tokenContract: RunnerToken = await tokenFactory.connect(deployer).deploy(
    skinContract.address,
    deployerAddress, // owner
    runnerTokenName, // name
    "IMR", // symbol
    "https://json-server-sgiz.onrender.com/token/", // baseURI
    "https://json-server-sgiz.onrender.com/token/", // contractURI
    operatorAllowlist, // operator allowlist
    deployerAddress, // royalty recipient
    ethers.BigNumber.from("2000"), // fee numerator
    gasOverrides, // tipOverrides
  );
  await tokenContract.deployed();
  console.log(`${runnerTokenName} contract deployed to ${tokenContract.address}`);
}

deploy().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
