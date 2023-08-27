import * as dotenv from 'dotenv';
import { getEnv } from '../libs/utils';

dotenv.config();

export default {
  alchemyApiKey: getEnv('ALCHEMY_API_KEY'),
  ethNetwork: getEnv('ETH_NETWORK'),
  client: {
    publicApiUrl: getEnv('PUBLIC_API_URL'),
    starkContractAddress: getEnv('STARK_CONTRACT_ADDRESS'),
    registrationContractAddress: getEnv('REGISTRATION_ADDRESS'),
    gasLimit: getEnv('GAS_LIMIT'),
    gasPrice: getEnv('GAS_PRICE'),
  },
  privateKey: getEnv('PRIVATE_KEY'),
  bulkMintMax: getEnv('BULK_MINT_MAX'),
  // Mint tokens
  tokenTokenAddress: getEnv('TOKEN_TOKEN_ADDRESS'),
  // Mint characters
  characterTokenAddress: getEnv('CHARACTER_TOKEN_ADDRESS'),
  // Mint skins
  skinTokenAddress: getEnv('SKIN_TOKEN_ADDRESS'),
};
