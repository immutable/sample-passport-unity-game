import { ethers } from "hardhat";
import { expect } from "chai";
import { RunnerToken, RunnerSkin, OperatorAllowlist__factory, RunnerSkin__factory, RunnerToken__factory } from "../typechain-types";

describe("RunnerToken", function () {
  let contract: RunnerToken;
  let skinContract: RunnerSkin;

  beforeEach(async function () {
    // get owner (first account)
    const [owner] = await ethers.getSigners();

    // deploy OperatorAllowlist contract
    const OperatorAllowlist = await ethers.getContractFactory(
      "OperatorAllowlist"
    ) as OperatorAllowlist__factory;
    const operatorAllowlist = await OperatorAllowlist.deploy(owner.address);

    // deploy RunnerSkin contract
    const RunnerSkin = await ethers.getContractFactory("RunnerSkin") as RunnerSkin__factory;
    skinContract = await RunnerSkin.deploy(
      owner.address, // owner
      "Immutable Runner Skin", // name
      "IMRS", // symbol
      "https://immutable.com/", // baseURI
      "https://immutable.com/", // contractURI
      operatorAllowlist.address, // operator allowlist contract
      owner.address, // royalty recipient
      ethers.BigNumber.from("2000") // fee numerator
    );
    await skinContract.deployed();

    // grant owner the minter role
    await skinContract.grantRole(await skinContract.MINTER_ROLE(), owner.address);

    // deploy RunnerToken contract
    const RunnerToken = await ethers.getContractFactory("RunnerToken") as RunnerToken__factory;
    contract = await RunnerToken.deploy(
      skinContract.address, // skin contract address
      owner.address, // owner
      "Immutable Runner Token", // name
      "IMR", // symbol
      "https://immutable.com/", // baseURI
      "https://immutable.com/", // contractURI
      operatorAllowlist.address, // operator allowlist contract
      owner.address, // royalty recipient
      ethers.BigNumber.from("2000") // fee numerator
    );
    await contract.deployed();

    // grant owner the minter role
    await contract.grantRole(await contract.MINTER_ROLE(), owner.address);
  });

  it("Should be deployed with the correct arguments", async function () {
    expect(await contract.name()).to.equal("Immutable Runner Token");
    expect(await contract.symbol()).to.equal("IMR");
    expect(await contract.baseURI()).to.equal("https://immutable.com/");
    expect(await contract.contractURI()).to.equal(
      "https://immutable.com/"
    );

    expect(await skinContract.name()).to.equal("Immutable Runner Skin");
    expect(await skinContract.symbol()).to.equal("IMRS");
    expect(await skinContract.baseURI()).to.equal("https://immutable.com/");
    expect(await skinContract.contractURI()).to.equal(
      "https://immutable.com/"
    );
  });

  it("Account with minter role should be able to mint next NFT", async function () {
    const [owner, recipient] = await ethers.getSigners();

    await contract.connect(owner).mintNextToken(recipient.address);
    expect(await contract.balanceOf(recipient.address)).to.equal(1);
    expect(await contract.ownerOf(1)).to.equal(recipient.address);

    await contract.connect(owner).mintNextToken(recipient.address);
    expect(await contract.balanceOf(recipient.address)).to.equal(2);
    expect(await contract.ownerOf(2)).to.equal(recipient.address);
  });

  it("Account with minter role should be able to mint next NFTs in batch", async function () {
    const [owner, recipient] = await ethers.getSigners();

    await contract.connect(owner).mintNextTokenByQuantity(recipient.address, 3);
    expect(await contract.balanceOf(recipient.address)).to.equal(3);
    expect(await contract.ownerOf(1)).to.equal(recipient.address);
    expect(await contract.ownerOf(2)).to.equal(recipient.address);
    expect(await contract.ownerOf(3)).to.equal(recipient.address);
  });

  it("Account without minter role should not be able to mint NFTs", async function () {
    const [_, acc1] = await ethers.getSigners();
    const minterRole = await contract.MINTER_ROLE();
    await expect(
      contract.connect(acc1).mint(acc1.address, 1)
    ).to.be.revertedWith(
      `AccessControl: account 0x70997970c51812dc3a010c7d01b50e0d17dc79c8 is missing role ${minterRole}`
    );
  });

  it("Account with three tokens can craft skin", async function () {
    const [owner, recipient] = await ethers.getSigners();

    await contract.connect(owner).mintNextTokenByQuantity(recipient.address, 3);
    expect(await contract.balanceOf(recipient.address)).to.equal(3);
    expect(await contract.ownerOf(1)).to.equal(recipient.address);
    expect(await contract.ownerOf(2)).to.equal(recipient.address);
    expect(await contract.ownerOf(3)).to.equal(recipient.address);

    await contract.connect(recipient).craftSkin(1, 2, 3);
    expect(await contract.balanceOf(recipient.address)).to.equal(0);
    expect(await skinContract.balanceOf(recipient.address)).to.equal(1);
    expect(await skinContract.ownerOf(1)).to.equal(recipient.address);
  });

  it("Should not be able to craft skin if given the duplicate token IDs", async function () {
    const [owner, recipient] = await ethers.getSigners();

    await contract.connect(owner).mintNextTokenByQuantity(recipient.address, 3);
    expect(await contract.balanceOf(recipient.address)).to.equal(3);
    expect(await contract.ownerOf(1)).to.equal(recipient.address);
    expect(await contract.ownerOf(2)).to.equal(recipient.address);
    expect(await contract.ownerOf(3)).to.equal(recipient.address);

    await expect(contract.connect(recipient).craftSkin(1, 2, 2)).to.be.revertedWith(
      `craftSkin: Token IDs must be different`
    );
  });

  it("Account which does not own tokens should not be able to craft skin", async function () {
    const [owner, recipient, recipient2] = await ethers.getSigners();

    await contract.connect(owner).mintNextTokenByQuantity(recipient.address, 3);
    expect(await contract.balanceOf(recipient.address)).to.equal(3);
    expect(await contract.ownerOf(1)).to.equal(recipient.address);
    expect(await contract.ownerOf(2)).to.equal(recipient.address);
    expect(await contract.ownerOf(3)).to.equal(recipient.address);

    await expect(contract.connect(recipient2).craftSkin(1, 2, 3)).to.be.revertedWith(
      `craftSkin: Caller does not own both tokens`
    );
  });
});