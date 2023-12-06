import { ethers } from "hardhat";
import { RunnerCharacter, RunnerCharacter__factory, RunnerSkin, RunnerSkin__factory, RunnerToken, RunnerToken__factory } from "../typechain-types";

async function deploy() {
  // get deployer
  const [deployer] = await ethers.getSigners();
  console.log("Deploying contracts with the account:", deployer.address);

  // check account balance
  console.log(
    "Account balance:",
    ethers.utils.formatEther(await deployer.getBalance())
  );

  const operatorAllowlist = process.env.OPERATOR_ALLOWLIST;
  if (operatorAllowlist === undefined) {
    throw new Error("Please set your OPERATOR_ALLOWLIST in a .env file");
  }

  // deploy MyERC721 contract
  const characterFactory: RunnerCharacter__factory = await ethers.getContractFactory(
    "RunnerCharacter"
  );
  const runnerCharacterName = "Immutable Runner Character"
  const contract: RunnerCharacter = await characterFactory.connect(deployer).deploy(
    deployer.address, // owner
    runnerCharacterName, // name
    "IMRC", // symbol
    "https://json-server-sgiz.onrender.com/character/", // baseURI
    "https://json-server-sgiz.onrender.com/character/", // contractURI
    operatorAllowlist, // operator allowlist
    deployer.address, // royalty recipient
    ethers.BigNumber.from("2000"), // fee numerator
  );
  await contract.deployed();

  // log deployed contract address
  console.log(`${runnerCharacterName} contract deployed to ${contract.address}`);

  const skinFactory: RunnerSkin__factory = await ethers.getContractFactory(
    "RunnerSkin"
  );
  const runnerSkinName = "Immutable Runner Skin"
  const skinContract: RunnerSkin = await skinFactory.connect(deployer).deploy(
    deployer.address, // owner
    runnerSkinName, // name
    "IMRS", // symbol
    "https://json-server-sgiz.onrender.com/skin/", // baseURI
    "https://json-server-sgiz.onrender.com/skin/", // contractURI
    operatorAllowlist, // operator allowlist
    deployer.address, // royalty recipient
    ethers.BigNumber.from("2000"), // fee numerator
  );
  await skinContract.deployed();

  // log deployed contract address
  console.log(`${runnerSkinName} contract deployed to ${skinContract.address}`);

  const tokenFactory: RunnerToken__factory = await ethers.getContractFactory(
    "RunnerToken"
  );
  const runnerTokenName = "Immutable Runner Token"
  const tokenContract: RunnerToken = await tokenFactory.connect(deployer).deploy(
    skinContract.address,
    deployer.address, // owner
    runnerTokenName, // name
    "IMR", // symbol
    "https://json-server-sgiz.onrender.com/token/", // baseURI
    "https://json-server-sgiz.onrender.com/token/", // contractURI
    operatorAllowlist, // operator allowlist
    deployer.address, // royalty recipient
    ethers.BigNumber.from("2000"), // fee numerator
  );
  await tokenContract.deployed();

  // log deployed contract address
  console.log(`${runnerTokenName} contract deployed to ${tokenContract.address}`);
}

deploy().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
