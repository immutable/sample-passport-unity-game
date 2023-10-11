// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

import "@imtbl/zkevm-contracts/contracts/token/erc721/preset/ImmutableERC721.sol";

contract RunnerToken is ImmutableERC721 {
    uint256 private _currentTokenId = 0;
    RunnerSkin private _skinContract;

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

    event AssetCrafted(
        address owner,
        uint256 tokenId1,
        uint256 tokenId2,
        uint256 newTokenId
    );

    function mintToken(address to, uint256 tokenId) public {
        _mintByID(to, tokenId);
        _currentTokenId++;
    }

    function mintTokenByID(address to, uint256 tokenId) public {
        _mintByID(to, tokenId);
        _currentTokenId++;
    }

    function mintNextToken(address to) public {
        uint256 tokenId = _currentTokenId + 1;
        _mintByID(to, tokenId);
        _currentTokenId++;
    }

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

        emit AssetCrafted(msg.sender, tokenId1, tokenId2, newTokenId);

        return newTokenId;
    }
}

contract RunnerSkin is ImmutableERC721 {
    uint256 private _currentTokenId = 0;

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

    event SkinCrafted(
        address owner,
        uint256 tokenId,
        uint256 newTokenId
    );

    function mintToken(address to, uint256 tokenId) public {
        _mintByID(to, tokenId);
        _currentTokenId++;
    }

    function mintTokenByID(address to, uint256 tokenId) public {
        _mintByID(to, tokenId);
        _currentTokenId++;
    }

    function mintNextToken(address to) public {
        uint256 tokenId = _currentTokenId + 1;
        _mintByID(to, tokenId);
        _currentTokenId++;
    }

    function craftSkin(uint256 tokenId) public returns (uint256) {
        require(ownerOf(tokenId) == msg.sender, "craftSkin: Caller does not own the token");
        
        uint256 newTokenId = _currentTokenId + 1;

        _burn(tokenId);
        
        mintTokenByID(msg.sender, newTokenId);

        emit SkinCrafted(msg.sender, tokenId, newTokenId);

        return newTokenId;
    }
}

contract RunnerCharacter is ImmutableERC721 {
    uint256 private _currentTokenId = 0;

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

    function mintNextToken(address to) public {
        uint256 tokenId = _currentTokenId + 1;
        _mintByID(to, tokenId);
        _currentTokenId++;
    }
}