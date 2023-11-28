// Copyright (c) Immutable Australia Pty Ltd 2018 - 2023
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@imtbl/zkevm-contracts/contracts/token/erc721/preset/ImmutableERC721.sol";

// The skin you manage to unlock and claim in the game
contract RunnerSkin is ImmutableERC721 {
    uint256 private _currentTokenId = 0;

    event SkinCrafted(address owner, uint256 newTokenId);

    constructor(
        address owner,
        string memory name,
        string memory symbol,
        string memory baseURI,
        string memory contractURI,
        address operatorAllowlist,
        address receiver,
        uint96 feeNumerator
    )
        ImmutableERC721(
            owner,
            name,
            symbol,
            baseURI,
            contractURI,
            operatorAllowlist,
            receiver,
            feeNumerator
        )
    {}

    function mintNextToken(address to) public returns (uint256) {
        uint256 tokenId = ++_currentTokenId;
        _mintByID(to, tokenId);
        return tokenId;
    }

    function craftSkin(uint256 tokenId) public returns (uint256) {
        require(ownerOf(tokenId) == msg.sender, "craftSkin: Caller does not own the token");
        _burn(tokenId);

        uint256 newTokenId = mintNextToken(msg.sender);

        emit SkinCrafted(msg.sender, newTokenId);

        return newTokenId;
    }
}