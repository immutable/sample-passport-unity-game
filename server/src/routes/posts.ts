import express from 'express';
import controller from '../controllers/posts';
const router = express.Router();

router.post('/mint/token', controller.mintToken);
router.post('/mint/character', controller.mintCharacter);
router.post('/mint/skin', controller.mintSkin);
router.get('/wallet', controller.wallet);
export = router;