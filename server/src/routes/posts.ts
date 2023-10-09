import express from 'express';
import controller from '../controllers/posts';
const router = express.Router();

router.post('/mint/token', controller.mintToken);
router.post('/mint/character', controller.mintCharacter);
router.post('/mint/skin', controller.mintSkin);
router.get('/wallet', controller.wallet);
router.post('/zkmint/token', controller.zkMintToken);
router.post('/zkmint/character', controller.zkMintCharacter);
router.post('/zkmint/skin', controller.zkMintSkin);
router.post('/zk/token/craftskin/encodeddata', controller.zkTokenCraftSkinData);
router.post('/zk/skin/craftskin/encodeddata', controller.zkSkinCraftSkinData);
export = router;