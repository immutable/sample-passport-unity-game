## How to deploy the contracts
1. Rename `server/.env.sample` to `.env`:
    1. Update `PRIVATE_KEY` with your wallet's private key 
    2. Update `ETHERSCAN_API_KEY` to your etherscan API key
2. Run `yarn install`
3. Run `yarn compile`
4. Run `yarn deploy`

Once successful, you should see the addresses of all the deployed contracts.