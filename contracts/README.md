## How to deploy the contracts
1. Rename `contracts/.env.example` to `.env`, and set all parameters with the appropriate data for the environment these Sample game contracts will be deployed.
2. Run `yarn install`
3. Run `yarn compile`
4. Run `yarn deploy`

If you use hardware ledger to deploy, remember to set NONCE_RESERVED_DEPLOYER_SECRET to be `ledger`.
Alternatively, it needs to be private key of the deployer. While it will work, it is not the best practice to use visible private key anywhere for deployment.
Once successful, you should see the addresses of all the deployed contracts.

## How to grant minter role to a different address
1. Rename `contracts/.env.example` to `.env`, and set all parameters with the appropriate data for the environment. This includes the `GRANT_MINTER_ROLE_CONTRACT_ADDRESS` and `GRANT_MINTER_ROLE_WALLET_ADDRESS`.
2. Run `yarn install`
3. Run `yarn compile`
4. Run `yarn grantMinterRole`