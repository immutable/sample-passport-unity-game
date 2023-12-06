<div align="center">
  <p align="center">
    <a  href="https://docs.x.immutable.com/docs">
      <img src="https://cdn.dribbble.com/users/1299339/screenshots/7133657/media/837237d447d36581ebd59ec36d30daea.gif" width="280"/>
    </a>
  </p>
</div>

---

# Sample Passport Unity Game

## Supported Platforms

* Windows
* Android
* iOS
* macOS

## Installation

> [!IMPORTANT]
> Some files are stored on Git Large File Storage, so you must download and install git-lfs from [here](https://git-lfs.github.com/).

1. Clone the [unity-immutable-sdk](https://github.com/immutable/unity-immutable-sdk) repository
2. Clone this repository inside the `unity-immutable-sdk` directory

Alternatively, you could change the path to the Immutable Passport package yourself in the [manifest](https://github.com/immutable/sample-passport-unity-game/blob/main/Packages/manifest.json) file (`"com.immutable.passport": "file:../../src/Packages/Passport"`).

## How to run the game

### Prerequisites
> [!IMPORTANT]  
> This game is set up to work on the Sandbox/Testnet environment only

1. Register the game by following the [Immutable X](https://docs.immutable.com/docs/x/sdks/unity#registering-your-game) or  [Immutable zkEVM](https://docs.immutable.com/docs/zkEVM/sdks/unity#registering-your-game) guide
    1. Make sure to set the [Callback URLs](https://docs.immutable.com/docs/x/sdks/unity#creating-an-oauth20-native-client) field to `imxsample://callback`
2. Open `Assets/Runner/Scripts/Constants.cs` and set the client ID

### Prerequisites for Windows and macOS only

To support minting and crafting in the game, three NFT collections need to be present:
1. **Immutable Runner Token (IMR)** for the Immutable coins you see in the game
2. **Immutabler Runner Character (IMRC)** for the Fox Runner character you use to play the game
3. **Immutable Runner Skin (IMRS)** for any skin you manage to unlock and claim

#### Immutable X

1. Follow [this](https://docs.immutable.com/docs/x/zero-to-hero-nft-minting) guide on how to create the three NFT collections
2. Rename `server/.env.sample` to `.env`:
    1. Update `PRIVATE_KEY` with your wallet's private key 
    2. Update `TOKEN_TOKEN_ADDRESS` to the contract address of Immutable Runner Token
    3. Update `CHARACTER_TOKEN_ADDRESS` to the contract address of Immutable Character Token
    4. Update `SKIN_TOKEN_ADDRESS` to the contract address of Immutable Skin Token
3. Open `Assets/Shared/Scripts/Service/ApiService.cs` 
    1. Update`TOKEN_TOKEN_ADDRESS` to the contract address of Immutable Runner Token
    2. Update`SKIN_TOKEN_ADDRESS` to the contract address of Immutable Skin Token
4. The game uses [Immutable X Single and Bulk transfer](https://docs.immutable.com/docs/x/sdks/unity/#immutable-x-transfer) to perform crafting, so you must [contact us](https://docs.immutable.com/docs/x/contact/) for pre-approval. You will need to provide us:
    1. The client ID
    2. The `TOKEN_TOKEN_ADDRESS` and the receiver's address `0x0000000000000000000000000000000000000000`
    3. The `SKIN_TOKEN_ADDRESS` and the receiver's address `0x0000000000000000000000000000000000000000`

#### Immutable zkEVM

1. [Set up your development wallet](https://docs.immutable.com/docs/zkEVM/guides/wallet)
2. [Obtain Test-$IMX](https://docs.immutable.com/docs/zkEVM/guides/faucet) so you can send transactions (specifically to mint assets in this game) and deploy smart contracts on zkEVM Testnet
3. Follow [this](https://docs.immutable.com/docs/zkEVM/deploy-contracts) guide on how to deploy the [three zkEVM contracts](https://github.com/immutable/sample-passport-unity-game/tree/main/contracts/contracts) the game requires
4. Rename `server/.env.sample` to `.env`:
    1. Update `PRIVATE_KEY` with your development wallet's private key
    2. Update `ZK_TOKEN_TOKEN_ADDRESS` to the contract address of Immutable Runner Token
    2. Update `ZK_CHARACTER_TOKEN_ADDRESS` to the contract address of Immutable Character Token
    3. Update `ZK_SKIN_TOKEN_ADDRESS` to the contract address of Immutable Skin Token
5. Open `Assets/Shared/Scripts/Service/ApiService.cs` 
    1. Update`ZK_TOKEN_TOKEN_ADDRESS` to the contract address of Immutable Runner Token
    2. Update`ZK_SKIN_TOKEN_ADDRESS` to the contract address of Immutable Skin Token
6. The game uses [zkEVM Send Transactions](https://docs.immutable.com/docs/zkEVM/sdks/unity#zkevm-send-transaction) to perform crafting, so you must [contact us](https://docs.immutable.com/docs/x/contact/) for pre-approval. You will need to provide us:
    1. The client ID
    2. The `TOKEN_TOKEN_ADDRESS` and the function signature of the [craftSkin](https://github.com/immutable/sample-passport-unity-game/blob/d003639beb8b6ae91dbb590a20349d4ba67e79b2/contracts/contracts/RunnerToken.sol#L57) method, i.e. `0x63daf310`
    3. The `SKIN_TOKEN_ADDRESS` and the function signature of the [craftSkin](https://github.com/immutable/sample-passport-unity-game/blob/d003639beb8b6ae91dbb590a20349d4ba67e79b2/contracts/contracts/RunnerSkin.sol#L52) method, i.e. `0xd820bf6a`

## Running the game on Windows and macOS

Run the server once pre-approved to use Immutable X Single/Bulk transfer and/or zkEVM Send Transactions for crafting.

The local server is used to mint and help craft assets.

1. Navigate to `server/`
2. Run `npm install`
3. Run `npm run dev`
4. Switch to the platform you would like to run on in Build Settings
5. Start the game
    1. If you are running the game inside the Editor, open `Assets/Shared/Scenes/Boot.unity`

## Running the game on Android and iOS

The game does not support minting and crafting on mobile, so a server is not required.

1. Switch to the platform you would like to run on in Build Settings
2. Build and run the game on your device


## Switching between Immutable X and zkEVM
On the main menu screen, there is a small checkbox at the top right that toggles between Immutable X and zkEVM. If checked, zkEVM will be used.

This flag is also saved in the game's `SaveManager`.

## Minting
The [local server](https://github.com/immutable/sample-passport-unity-game/tree/main/server) is used to mint assets the gamer collects directly to their wallet. See endpoints `/mint/*` for Immutable X and `zkmint/*` for zkEVM.

## Crafting

### Immutable X
Crafting is done by using the Immutable X Single and Bulk transfer functions.

Required asset(s) to craft a new asset is transferred to `0x0000000000000000000000000000000000000000` to burn them. Once that is successful, the new asset is minted to the gamer's wallet using the local server.

For example, to claim the first skin, the user needs to burn three of their tokens:
* [Burn three tokens](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L504) using Immutable X Bulk Transfer
* [Mint skin](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L507) using the [`POST /mint/skin`](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/server/src/routes/posts.ts#L7)

### zkEVM
Crafting is done by calling the craft function in the smart contract. This is done by using zkEVM Send Transaction.

For example, to claim the cooler skin, the user needs to burn their current skin first:
* [Get the encoded data](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L557) required to call the RunnerSkin contract's [`craftSkin` function](https://github.com/immutable/sample-passport-unity-game/blob/d003639beb8b6ae91dbb590a20349d4ba67e79b2/contracts/contracts/RunnerSkin.sol#L52) by calling [`POST /zk/skin/craftskin/encodeddata`](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/server/src/routes/posts.ts#L13)
* Use zkEVM Send Transaction to [call the `craftSkin` function](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L558) by passing the RunnerSkin contract address and the encoded data
  * Note that the craftSkin function does all the burning and minting