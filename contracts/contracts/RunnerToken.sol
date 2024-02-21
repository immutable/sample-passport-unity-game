// Copyright (c) Immutable Australia Pty Ltd 2018 - 2023
// SPDX-License-Identifier: MIT
pragma solidity 0.8.19;

import "@imtbl/contracts/contracts/token/erc721/preset/ImmutableERC721.sol";
import "./RunnerSkin.sol";

// The Immutable coins you collect in the game
contract RunnerToken is ImmutableERC721 {
    uint256 private _currentTokenId = 0;
    RunnerSkin private _skinContract;

    event SkinCrafted(address owner, uint256 newTokenId);

    constructor(
        address skinContractAddr,
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
    {
        _skinContract = RunnerSkin(skinContractAddr);
    }

    // Mints the next token
    function mintNextToken(address to) public returns (uint256) {
        uint256 tokenId = ++_currentTokenId;
        _mintByID(to, tokenId);
        return tokenId;
    }

    // Mints number of tokens specified
    function mintNextTokenByQuantity(address to, uint256 quantity) public {
        for (uint256 i = 0; i < quantity; i++) {
            _mintByID(to, ++_currentTokenId);
        }
    }

    // Burns the three tokens specified and crafts a skin to the caller
    function craftSkin(
        uint256 tokenId1,
        uint256 tokenId2,
        uint256 tokenId3
    ) public returns (uint256) {
        require(tokenId1 != tokenId2, "craftSkin: Token IDs must be different");
        require(tokenId1 != tokenId3, "craftSkin: Token IDs must be different");
        require(tokenId2 != tokenId3, "craftSkin: Token IDs must be different");

        require(
            ownerOf(tokenId1) == msg.sender && ownerOf(tokenId2) == msg.sender && ownerOf(tokenId3) == msg.sender,
            "craftSkin: Caller does not own both tokens"
        );
        
        uint256 newTokenId = _currentTokenId + 1;

        _burn(tokenId1);
        _burn(tokenId2);
        _burn(tokenId3);
        
        _skinContract.mintNextToken(msg.sender);

        emit SkinCrafted(msg.sender, newTokenId);

        return newTokenId;
    }
}