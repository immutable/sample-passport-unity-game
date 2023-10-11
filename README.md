<div align="center">
  <p align="center">
    <a  href="https://docs.x.immutable.com/docs">
      <img src="https://cdn.dribbble.com/users/1299339/screenshots/7133657/media/837237d447d36581ebd59ec36d30daea.gif" width="280"/>
    </a>
  </p>
</div>

---

# Sample Passport Unity Game

## Prerequisites
- [git-lfs](https://git-lfs.github.com/): since large image files are stored on Git Large File Storage, you must download and install git-lfs from [here](https://git-lfs.github.com/).

## Installation

1. Clone the [unity-immutable-sdk](https://github.com/immutable/unity-immutable-sdk) repository
2. Clone this repository inside the `unity-immutable-sdk` directory

Alternatively, you could change the path to the Immutable Passport package yourself in the [manifest](https://github.com/immutable/sample-passport-unity-game/blob/main/Packages/manifest.json) file (`"com.immutable.passport": "file:../../src/Packages/Passport"`).

## Quickstart for Windows and MacOS only

1. Navigate to `server/`
2. Run `npm install`
3. Run `npm run dev`
4. Start the game

## Supported Platforms

* Windows
* Android
* iOS
* MacOS

## SDK Features Implemented in Game

* Connect to Passport
* Log out of Passport
* Get email
* Checks if there are any credentials saved

### Windows and MacOS only

* Immutable X Single transfer
* Immutable X Bulk transfer
* zkEVM Send transaction

![](https://github.com/immutable/sample-passport-unity-game/blob/main/demo.gif)