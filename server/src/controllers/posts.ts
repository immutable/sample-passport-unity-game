import { Request, Response, NextFunction } from 'express';
import { AlchemyProvider, JsonRpcProvider } from '@ethersproject/providers';
import { Wallet } from '@ethersproject/wallet';
import { ethers } from 'ethers';
import { createStarkSigner, generateLegacyStarkPrivateKey, Configuration } from '@imtbl/core-sdk';
import { config, provider as imxProvider, immutablexClient as imxClient } from '@imtbl/sdk';
import env from '../config/client';
const baseConfig = new config.ImmutableConfiguration({
    environment: config.Environment.SANDBOX
  });
const client = new imxClient.ImmutableXClient({baseConfig: baseConfig});

const provider = new AlchemyProvider(env.ethNetwork, env.alchemyApiKey);
const zkEvmProvider = new JsonRpcProvider('https://rpc.testnet.immutable.com');

const waitForTransaction = async (promise: Promise<string>) => {
    const txId = await promise;
    console.log('Waiting for transaction', {
      txId,
      etherscanLink: `https://goerli.etherscan.io/tx/${txId}`,
      alchemyLink: `https://dashboard.alchemyapi.io/mempool/eth-goerli/tx/${txId}`,
    });
    const receipt = await provider.waitForTransaction(txId);
    if (receipt.status === 0) {
      throw new Error('Transaction rejected');
    }
    console.log(`Transaction Mined: ${receipt.blockNumber}`);
    return receipt;
};

const mintToken = async (req: Request, res: Response, next: NextFunction) => {
    return mint(env.tokenTokenAddress, req, res, next);
}

const mintCharacter = async (req: Request, res: Response, next: NextFunction) => {
    return mint(env.characterTokenAddress, req, res, next);
}

const mintSkin = async (req: Request, res: Response, next: NextFunction) => {
    return mint(env.skinTokenAddress, req, res, next);
}

const mint = async (tokenAddress: string, req: Request, res: Response, next: NextFunction) => {
    try {
        let wallet: string = req.body.toUserWallet ?? null;
        console.log(`To user: ${wallet}`);
        let number = parseInt(req.body.number ?? "1");

        const signer = new Wallet(env.privateKey).connect(provider);
        const pk = await generateLegacyStarkPrivateKey(signer);
        const starkSigner = createStarkSigner(pk);
        // Setting environment to just sandbox doesn't work
        // so need to override instead
        const overrides = {
            immutableXConfig: {
                apiConfiguration: new Configuration(),
                ethConfiguration: {
                    coreContractAddress: '0x7917eDb51ecD6CdB3F9854c3cc593F33de10c623',
                    registrationContractAddress: '0x1C97Ada273C9A52253f463042f29117090Cd7D83',
                    chainID: 5
                }
            }
        };
        const minter = new imxProvider.GenericIMXProvider(
            new imxProvider.ProviderConfiguration({
                baseConfig,
                overrides
            }),
            signer,
            starkSigner
          );
        // Get latest token ID
        var tokenId = 1;
        const mintsResponse = await client.listMints({
            pageSize: 1,
            tokenAddress: tokenAddress,
            orderBy: 'created_at'
        });
        if (mintsResponse.result.length > 0) {
            const latestTokenId = mintsResponse.result[0].token.data.token_id;
            if (latestTokenId) {
                tokenId = parseInt(latestTokenId, 10) + 1;
            } else {
                return res.status(500).json({
                    message: "Failed to mint token to user: could not get the latest token ID"
                });
            }
        }
        console.log('tokenId');
        console.log(tokenId);

        const registerImxResult = await minter.registerOffchain();

        if (registerImxResult.tx_hash === '') {
            console.log('Minter registered, continuing...');
        } else {
            console.log('Waiting for minter registration...');
            await waitForTransaction(Promise.resolve(registerImxResult.tx_hash));
        }

        console.log(`OFF-CHAIN MINT ${number} NFTS`);

        const tokens = Array.from({ length: number }, (_, i) => i).map(i => ({
            id: (tokenId + i).toString(),
            blueprint: 'onchain-metadata',
        }));

        const result = await client.mint(signer, {
            contract_address: tokenAddress,
            users: [
                {
                    tokens,
                    user: wallet.toLowerCase()
                }
            ]
        });
        console.log(result);

        // return response
        return res.status(200).json({
        });
    } catch (error) {
        console.log(error);
        return res.status(400).json({
            message: "Failed to mint token to user"
        });
    }
};

const wallet = async (req: Request, res: Response, next: NextFunction) => {
    try {
        let address = req.query.user as string;
        // Get tokens
        const tokens = await client.listAssets({
            user: address,
            pageSize: 100,
            collection: env.tokenTokenAddress
        });
        const tokensJson: any[] = [];
        tokens.result.forEach(function (token: { token_address: string; token_id: string; }) {
            tokensJson.push({
                tokenAddress: token.token_address,
                tokenId: token.token_id
            })
        });
        // Get characters
        const characters = await client.listAssets({
            user: address,
            pageSize: 100,
            collection: env.characterTokenAddress
        });
        const charactersJson: any[] = [];
        characters.result.forEach(function (character: { token_address: string; token_id: string; }) {
            charactersJson.push({
                tokenAddress: character.token_address,
                tokenId: character.token_id
            })
        });
        // Get skins
        const skins = await client.listAssets({
            user: address,
            pageSize: 100,
            collection: env.skinTokenAddress
        });
        const skinsJson: any[] = [];
        skins.result.forEach(function (skin: { token_address: string; token_id: string; }) {
            skinsJson.push({
                tokenAddress: skin.token_address,
                tokenId: skin.token_id
            })
        });

        return res.status(200).json({
            tokens: tokensJson,
            characters: charactersJson,
            skins: skinsJson
        });
    } catch (error) {
        console.log(error);
        return res.status(400).json({
            message: "Failed to get user's assets"
        });
    }
};

const zkMintToken = async (req: Request, res: Response, next: NextFunction) => {
    return zkMint(env.zkTokenTokenAddress, req, res, next);
}

const zkMintCharacter = async (req: Request, res: Response, next: NextFunction) => {
    return zkMint(env.zkCharacterTokenAddress, req, res, next);
}

const zkMintSkin = async (req: Request, res: Response, next: NextFunction) => {
    return zkMint(env.zkSkinTokenAddress, req, res, next);
}

const zkMint = async (tokenAddress: string, req: Request, res: Response, next: NextFunction) => {
    try {
        let wallet: string = req.body.toUserWallet ?? null;
        console.log(`To user: ${wallet}`);
        let number = parseInt(req.body.number ?? "1");

        const signer = new Wallet(env.privateKey).connect(zkEvmProvider);

        const abi = [
            'function grantMinterRole(address user) public',
            'function mintNextToken(address too) public'
        ];
    
        const contract = new ethers.Contract(tokenAddress, abi, signer);

        const grantTx = await contract.grantMinterRole('0xEED04A543eb26cB79Fc41548990e105C08B16464');
        await grantTx.wait();

        for (let i = 0; i < number; i++)
        {
            const tx = await contract.mintNextToken(wallet);
            await tx.wait();
        }

        // return response
        return res.status(200).json({
        });
    } catch (error) {
        console.log(error);
        return res.status(400).json({
            message: "Failed to mint token to user"
        });
    }
};

const zkTokenCraftSkinData = async (req: Request, res: Response, next: NextFunction) => {
    try {
        let tokenId1: string = req.body.tokenId1 ?? null;
        let tokenId2: string = req.body.tokenId2 ?? null;
        let tokenId3: string = req.body.tokenId3 ?? null;
        console.log(`Token IDs: [${tokenId1}, ${tokenId2}, ${tokenId3}]`);

        const signer = new Wallet(env.privateKey).connect(zkEvmProvider);

        const abi = [
            'function craftSkin(uint256 tokenId1, uint256 tokenId2, uint256 tokenId3) public returns (uint256)'
        ];
        let iface = new ethers.utils.Interface(abi);
        let encodedData = iface.encodeFunctionData('craftSkin', [tokenId1, tokenId2, tokenId3]);
        console.log(`encodedData: ${encodedData}`);

        return res.status(200).json({
            data: encodedData
        });
    } catch (error) {
        console.log(error);
        return res.status(400).json({
            message: "Failed to mint token to user"
        });
    }
};

const zkSkinCraftSkinData = async (req: Request, res: Response, next: NextFunction) => {
    try {
        let tokenId: string = req.body.tokenId ?? null;
        console.log(`Token ID: ${tokenId}`);

        const signer = new Wallet(env.privateKey).connect(zkEvmProvider);

        const abi = [
            'function craftSkin(uint256 tokenId) public returns (uint256)'
        ];
        let iface = new ethers.utils.Interface(abi);
        let encodedData = iface.encodeFunctionData('craftSkin', [tokenId]);
        console.log(`encodedData: ${encodedData}`);
        
        return res.status(200).json({
            data: encodedData
        });
    } catch (error) {
        console.log(error);
        return res.status(400).json({
            message: "Failed to mint token to user"
        });
    }
};

export default { mintToken, mintCharacter, mintSkin, wallet, zkMintToken, zkMintCharacter, zkMintSkin, zkTokenCraftSkinData, zkSkinCraftSkinData };