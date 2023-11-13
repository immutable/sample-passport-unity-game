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

## Prerequisites
- Unity Editor 2021.3.26f1 (but newer versions should work too)

## Installation

> :clipboard: Prerequisites
>
>[git-lfs](https://git-lfs.github.com/): since `.dll` files are stored on Git Large File Storage, you must download and install git-lfs from [here](https://git-lfs.github.com/).

1. Clone the [unity-immutable-sdk](https://github.com/immutable/unity-immutable-sdk) repository
2. Clone this repository inside the `unity-immutable-sdk` directory

Alternatively, you could change the path to the Immutable Passport package yourself in the [manifest](https://github.com/immutable/sample-passport-unity-game/blob/main/Packages/manifest.json) file (`"com.immutable.passport": "file:../../src/Packages/Passport"`).

## Quickstart for Windows and MacOS only

The game requires a local server to mint and craft assets.

1. Navigate to `server/`
2. Run `npm install`
3. Run `npm run dev`
4. Start the game

## SDK Features Implemented in Game

* Connect to Passport
* Log out of Passport
* Get email
* Checks if there are any credentials saved

The following features are implemented only for Windows and macOS targets, as they rely on a local server (as described [here](#quickstart-for-windows-and-macos-only)):

* Immutable X Single Transfer
* Immutable X Bulk Transfer
* zkEVM Send Transaction

## Switching between Immutable X and zkEVM
On the main menu screen, there is a small checkbox at the top right that toggles between Immutable X and zkEVM. If checked, zkEVM will be used.

This flag is also saved in the game's `SaveManager`.

## Minting
The [local server](https://github.com/immutable/sample-passport-unity-game/tree/main/server) is used to mint assets collected by the gamer directly to their wallet. See endpoints `/mint/*` for Immutable X and `zkmint/*` for zkEVM.

## Crafting

### Immutable X
Crafting is done by using the Immutable X Single and Bulk transfer functions.

Required asset(s) to craft a new asset is transferred to `0x0000000000000000000000000000000000000000` to burn them. Once that is successful, the new asset is minted to the gamer's wallet using the local server.

For example, to claim the first skin, the user needs to burn three of their tokens:
* [Burn three tokens](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L504) using Immutable X Bulk Transfer
* [Mint skin](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L507) using the `/mint/skin` [endpoint](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/server/src/routes/posts.ts#L7)

### zkEVM
Crafting is done by calling the craft function in the smart contract. This is done by using zkEVM Send Transaction.

For example, to claim the cooler skin, the user needs to burn their current skin first:
* [Get the encoded data](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L557) required to call the smart contract [craft skin function](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/server/src/contracts/RunnerToken.sol#L133) by calling the local server `/zk/skin/craftskin/encodeddata` [endpoint](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/server/src/routes/posts.ts#L13)
* [Call the craft skin function](https://github.com/immutable/sample-passport-unity-game/blob/ed06ce54c77d0e53837e494ae3acb04b4e98f7df/Assets/Shared/Scripts/UI/LevelCompleteScreen.cs#L558) by using zkEVM Send Transaction
  * Note that the `craftSkin` function does all the burning and minting
 
## zkEVM Smart Contracts
The zkEVM smart contracts are [here](https://github.com/immutable/sample-passport-unity-game/blob/main/server/src/contracts/RunnerToken.sol).
* `RunnerToken` is for the Immutable coins you see in the game
* `RunnerCharacter` is for the fox runner character you use to play the game
* `RunnerSkin` is for any skin you manager to unlock and claim



![](https://github.com/immutable/sample-passport-unity-game/blob/main/demo.gif)
