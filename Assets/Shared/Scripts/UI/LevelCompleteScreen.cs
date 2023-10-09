using System.ComponentModel;
using HyperCasual.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Immutable.Passport;
using Immutable.Passport.Model;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace HyperCasual.Runner
{
    /// <summary>
    /// This View contains celebration screen functionalities
    /// </summary>
    public class LevelCompleteScreen : View
    {
        [SerializeField]
        HyperCasualButton m_NextButton;
        [SerializeField]
        HyperCasualButton m_ContinuePassportButton;
        [SerializeField]
        Image[] m_Stars;
        [SerializeField]
        AbstractGameEvent m_NextLevelEvent;
        [SerializeField]
        TextMeshProUGUI m_GoldText;
        [SerializeField]
        Slider m_XpSlider;
        [SerializeField]
        GameObject m_Loading;

        [SerializeField]
        GameObject m_CompletedContainer;
        [SerializeField]
        GameObject m_MintedContainer;
        [SerializeField]
        GameObject m_SkinUnlockedContainer;
        [SerializeField]
        GameObject m_BonusSkinContainer;

        // Minted Fox
        [SerializeField]
        TextMeshProUGUI m_MintedTitle;
        [SerializeField]
        HyperCasualButton m_MintedNextButton;

        // Unlocked Skin
        [SerializeField]
        TextMeshProUGUI m_SkinUnlockedErrorMessage;
        [SerializeField]
        HyperCasualButton m_SkinUnlockedGetButton;
        [SerializeField]
        HyperCasualButton m_SkinUnlockedNextButton;
        [SerializeField]
        GameObject m_SkinUnlockedLoading;

        // Bonus
        [SerializeField]
        HyperCasualButton m_BonusSkinGetButton;
        [SerializeField]
        HyperCasualButton m_BonusSkinNextButton;
        [SerializeField]
        GameObject m_BonusSkinLoading;
        [SerializeField]
        TextMeshProUGUI m_BonusSkinMessage;
        
        /// <summary>
        /// The slider that displays the XP value 
        /// </summary>
        public Slider XpSlider => m_XpSlider;

        int m_GoldValue;

        private ConnectResponse? ConnectResponse;

        private ApiService Api = new();
        private bool MintingTokens = false;
        private string? Address;
        
        /// <summary>
        /// The amount of gold to display on the celebration screen.
        /// The setter method also sets the celebration screen text.
        /// </summary>
        public int GoldValue
        {
            get => m_GoldValue;
            set
            {
                if (m_GoldValue != value)
                {
                    m_GoldValue = value;
                    m_GoldText.text = GoldValue.ToString();
                }
            }
        }

        float m_XpValue;
        
        /// <summary>
        /// The amount of XP to display on the celebration screen.
        /// The setter method also sets the celebration screen slider value.
        /// </summary>
        public float XpValue
        {
            get => m_XpValue;
            set
            {
                if (!Mathf.Approximately(m_XpValue, value))
                {
                    m_XpValue = value;
                    m_XpSlider.value = m_XpValue;
                }
            }
        }

        int m_StarCount = -1;
        
        /// <summary>
        /// The number of stars to display on the celebration screen.
        /// </summary>
        public int StarCount
        {
            get => m_StarCount;
            set
            {
                if (m_StarCount != value)
                {
                    m_StarCount = value;
                    DisplayStars(m_StarCount);
                }
            }
        }

        public async void OnEnable()
        {
            var connected = await Passport.Instance.HasCredentialsSaved();
            HideLoading();
            ShowContinueWithPassportButton(!connected);
            ShowNextButton(connected);
            ShowCompletedContainer(true);
            ShowMintedContainer(false);
            ShowSkinUnlockedContainer(false);
            ShowBonusSkinContainer(false);

            // Continue with Passport button
            m_ContinuePassportButton.RemoveListener(OnContinueButtonClicked);
            m_ContinuePassportButton.AddListener(OnContinueButtonClicked);

            // Burn to get new skin button
            m_SkinUnlockedGetButton.RemoveListener(OnBurnTokens);
            m_SkinUnlockedGetButton.AddListener(OnBurnTokens);

            // Get bonus skin button
            m_BonusSkinGetButton.RemoveListener(BurnSkinForCoolerSkin);
            m_BonusSkinGetButton.AddListener(BurnSkinForCoolerSkin);

            // Next level button on Completed screen
            m_NextButton.RemoveListener(OnNextButtonClicked);
#if UNITY_STANDALONE_WIN
            // Show new skin when level 2 completes
            // And user is not already using it
            if (MemoryCache.CurrentLevel == 2 && !MemoryCache.UseNewSkin)
            {
                m_NextButton.AddListener(OnNextButtonClickedShowNewSkin);
            }
            // Show bonus skin when level 3 completes
            // And user already uses the new skin
            // And user is not using cooler skin
            else if (MemoryCache.CurrentLevel == 3 && MemoryCache.UseNewSkin && !MemoryCache.UseCoolerSkin)
            {
                m_NextButton.AddListener(OnNextButtonClickedShowBonusSkin);
            }
            else
            {
                m_NextButton.AddListener(OnNextButtonClicked);
            }
#else
            m_NextButton.AddListener(OnNextButtonClicked);
#endif

            // Next level button on Minted Fox (and Tokens) screen
            m_MintedNextButton.RemoveListener(OnNextButtonClicked);
            m_MintedNextButton.AddListener(OnNextButtonClicked);

            // Next level button on New Skin Unlocked screen
            m_SkinUnlockedNextButton.RemoveListener(OnNextButtonClicked);
            m_SkinUnlockedNextButton.AddListener(OnNextButtonClicked);

            // Next/Pass button on bonus skin screen
            m_BonusSkinNextButton.RemoveListener(OnNextButtonClicked);
            m_BonusSkinNextButton.AddListener(OnNextButtonClicked);

#if UNITY_STANDALONE_WIN
            // If the user is connected to Passport, try and mint the tokens in the background
            // if minting did not complete in time, we just ignore any errors
            if (connected)
            {
                MintTokens();
            }
#endif
        }

        void OnNextButtonClicked()
        {
            Debug.Log("On Next Button Clicked");
            // Reset screens
            ShowCompletedContainer(true);
            ShowMintedContainer(false);
            ShowSkinUnlockedContainer(false);
            ShowBonusSkinContainer(false);

            m_NextLevelEvent.Raise();
        }

        async void OnContinueButtonClicked()
        {
            try 
            {
                ShowContinueWithPassportButton(false);
                ShowLoading();

#if UNITY_ANDROID || UNITY_IPHONE
                await Passport.Instance.ConnectPKCE();
#else
                await Passport.Instance.Connect();
#endif

                // If in zkEVM mode, connect to EVM
                if (SaveManager.Instance.ZkEvm)
                {
                    await Passport.Instance.ConnectEvm();
                    // Get address and save it
                    List<string> accounts = await Passport.Instance.ZkEvmRequestAccounts();
                    Address = accounts[0];
                }
                else
                {
                    // Get address and save it
                    Address = await Passport.Instance.GetAddress();
                }

#if UNITY_STANDALONE_WIN
                // Successfully connected                
                MintFoxAndTokens();
#else
                ShowContinueWithPassportButton(false);
                HideLoading();
                ShowNextButton(true);
#endif
            } catch (Exception ex)
            {
                Debug.Log($"Failed to connect: {ex.Message}");
                ShowContinueWithPassportButton(false);
                HideLoading();
                ShowNextButton(true);
            }
        }

        void OnNextButtonClickedShowNewSkin()
        {
            Debug.Log("On Next Button Clicked Show New Skin");
            ShowCompletedContainer(false);
            ShowMintedContainer(false);
            ShowSkinUnlockedContainer(true);
            ShowBonusSkinContainer(false);
        }

        void OnNextButtonClickedShowBonusSkin()
        {
            Debug.Log("On Next Button Clicked Show Bonus Skin");
            ShowCompletedContainer(false);
            ShowMintedContainer(false);
            ShowSkinUnlockedContainer(false);
            ShowBonusSkinContainer(true);
        }

        void ShowCompletedContainer(bool show)
        {
            m_CompletedContainer.gameObject.SetActive(show);
        }

        void ShowMintedContainer(bool show)
        {
            m_MintedContainer.gameObject.SetActive(show);
        }

        void ShowSkinUnlockedContainer(bool show)
        {
            m_SkinUnlockedContainer.gameObject.SetActive(show);
            if (show)
            {
                // Reset all other views
                m_SkinUnlockedLoading.gameObject.SetActive(false);
                m_SkinUnlockedErrorMessage.gameObject.SetActive(false);
                m_SkinUnlockedGetButton.gameObject.SetActive(true);
            }
        }

        void ShowBonusSkinContainer(bool show)
        {
            m_BonusSkinContainer.gameObject.SetActive(show);
            if (show)
            {
                // Reset all other views
                m_BonusSkinLoading.gameObject.SetActive(false);
                ShowBonusSkinMessage("Burn your current skin\nfor a cooler skin!");
                m_BonusSkinGetButton.gameObject.SetActive(true);
            }
        }

        void ShowContinueWithPassportButton(bool show)
        {
            m_ContinuePassportButton.gameObject.SetActive(show);
        }

        void ShowNextButton(bool show)
        {
            m_NextButton.gameObject.SetActive(show);
        }

        void ShowLoading()
        {
            m_Loading.gameObject.SetActive(true);
        }

        void HideLoading()
        {
            m_Loading.gameObject.SetActive(false);
        }

        public void ConfirmCode()
        {
            if (ConnectResponse != null)
            {
                Application.OpenURL(ConnectResponse.Url);
            }
        }

        void DisplayStars(int count)
        {
            count = Mathf.Clamp(count, 0, m_Stars.Length);

            if (m_Stars.Length > 0 && count >= 0 && count <= m_Stars.Length)
            {
                for (int i = 0; i < m_Stars.Length; i++)
                {
                    m_Stars[i].gameObject.SetActive(i < count);
                }
            }
        }

        private async UniTask GetWalletAddress()
        {
            if (Address == null)
            {
                if (SaveManager.Instance.ZkEvm)
                {
                    List<string> accounts = await Passport.Instance.ZkEvmRequestAccounts();
                    Address = accounts[0];
                }
                else
                {
                    Address = await Passport.Instance.GetAddress();
                }
            }
        }

        public void OnWalletClicked()
        {
            if (Address != null)
            {
                if (SaveManager.Instance.ZkEvm)
                {
                    Application.OpenURL($"https://explorer.testnet.immutable.com/address/{Address}");
                }
                else 
                {
                    Application.OpenURL($"http://localhost:6060/wallet?user={Address}");
                }
            }
        }

        private async void MintFoxAndTokens()
        {
            // Mint fox
            bool mintedFox = await Api.MintFox(Address);

            if (StarCount > 0)
            {
                // User collected tokens
                bool mintedTokens = await Api.MintTokens(StarCount, Address);

                if (!mintedFox && !mintedTokens)
                {
                    Debug.Log($"Failed to mint both a fox and tokens");
                    ShowContinueWithPassportButton(false);
                    HideLoading();
                    ShowNextButton(true);
                }
                else
                {
                    string numTokens = StarCount == 1 ? "a token" : $"{StarCount} tokens";
                    if (mintedFox && mintedTokens)
                    {
                        m_MintedTitle.text = $"You now own a Fox and {numTokens}!";
                    }
                    else if (mintedFox && !mintedTokens)
                    {
                        m_MintedTitle.text = $"You now own a Fox!";
                    }
                    else if (!mintedFox && mintedTokens)
                    {
                        m_MintedTitle.text = $"You now own {numTokens}";
                    }
                    HideLoading();
                    ShowCompletedContainer(false);
                    ShowMintedContainer(true);
                }
            }
            else
            {
                // User did not collect and tokens
                if (mintedFox)
                {
                    m_MintedTitle.text = $"You now own a Fox!";
                    HideLoading();
                    ShowCompletedContainer(false);
                    ShowMintedContainer(true);
                }
                else
                {
                    Debug.Log($"Failed to mint a fox");
                    ShowContinueWithPassportButton(false);
                    HideLoading();
                    ShowNextButton(true);
                }
            }
        }

        private async void MintTokens()
        {
            if (!MintingTokens)
            {
                MintingTokens = true;
                try
                {
                    await GetWalletAddress();
                    if (StarCount > 0)
                    {
                        bool mintedTokens = await Api.MintTokens(StarCount, Address);
                        Debug.Log(mintedTokens ? "Minted tokens" : $"Failed to mint tokens");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error minting tokens: {ex.Message}");
                }
                MintingTokens = false;
            }
        }

        private async void OnBurnTokens()
        {
            Debug.Log("On Burn Tokens...");
            m_SkinUnlockedLoading.gameObject.SetActive(true);
            m_SkinUnlockedGetButton.gameObject.SetActive(false);

            await GetWalletAddress();
            List<TokenModel> tokens = await Api.GetTokens(3, Address);
            if (tokens.Count >= 3)
            {
                try
                {
                    if (SaveManager.Instance.ZkEvm)
                    {
                        string encodedData = await Api.GetTokenCraftSkinEcodedData(tokens[0].token_id, tokens[1].token_id, tokens[2].token_id);
                        string transactionHash = await Passport.Instance.ZkEvmSendTransaction(new TransactionRequest() {
                            To = ApiService.ZK_TOKEN_TOKEN_ADDRESS,
                            Data = encodedData,
                            Value = "0"
                        });
                        Debug.Log($"Transaction hash: {transactionHash}");
                        MemoryCache.UseNewSkin = true;
                        ShowSkinUnlockedMessage("You now have the skin and can use it!");
                    }
                    else
                    {
                        // Burn tokens
                        List<NftTransferDetails> transferDetails = new List<NftTransferDetails>();
                        tokens.ForEach(delegate (TokenModel token)
                        {
                            Debug.Log($"Got token ID: {token.token_id}");
                            transferDetails.Add(new NftTransferDetails(
                                "0x0000000000000000000000000000000000000000",
                                token.token_id,
                                ApiService.TOKEN_TOKEN_ADDRESS.ToLower()));
                        });
                        var response = await Passport.Instance.ImxBatchNftTransfer(transferDetails.ToArray());

                        // Mint skin
                        bool mintedSkin = await Api.MintSkin(Address);
                        if (mintedSkin)
                        {

                            MemoryCache.UseNewSkin = true;
                            ShowSkinUnlockedMessage("You now have the skin and can use it!");
                        }
                        else
                        {
                            Debug.Log($"Something went wrong while minting skin");
                            ShowSkinUnlockedMessage("Something went wrong :(");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Something went wrong while burning tokens {ex.Message}");
                    ShowSkinUnlockedMessage("Something went wrong :(");
                }
            }
            else 
            {
                ShowSkinUnlockedMessage("Not enough tokens to burn");
            }
        }

        private void ShowSkinUnlockedMessage(string message)
        {
            m_SkinUnlockedErrorMessage.text = message;
            m_SkinUnlockedErrorMessage.gameObject.SetActive(true);
            m_SkinUnlockedGetButton.gameObject.SetActive(false);
            m_SkinUnlockedLoading.gameObject.SetActive(false);
        }

        private async void BurnSkinForCoolerSkin()
        {
            Debug.Log("On Burn Skin For Cooler Skins...");
            m_BonusSkinLoading.gameObject.SetActive(true);
            m_BonusSkinGetButton.gameObject.SetActive(false);

            await GetWalletAddress();
            List<TokenModel> tokens = await Api.GetSkin(Address);
            if (tokens.Count > 0)
            {
                try
                {
                    if (SaveManager.Instance.ZkEvm)
                    {
                        string encodedData = await Api.GetSkinCraftSkinEcodedData(tokens[0].token_id);
                        string transactionHash = await Passport.Instance.ZkEvmSendTransaction(new TransactionRequest() {
                            To = ApiService.ZK_SKIN_TOKEN_ADDRESS,
                            Data = encodedData,
                            Value = "0"
                        });
                        Debug.Log($"Transaction hash: {transactionHash}");
                        MemoryCache.UseCoolerSkin = true;
                        ShowBonusSkinMessage("You now have a cooler skin and can use it!");
                    }
                    else 
                    {
                        // Burn skin
                        TokenModel skin = tokens[0];
                        var response = await Passport.Instance.ImxTransfer(
                            UnsignedTransferRequest.ERC721(
                                "0x0000000000000000000000000000000000000000",
                                skin.token_id,
                                ApiService.SKIN_TOKEN_ADDRESS
                            )
                        );

                        // Mint skin
                        bool mintedSkin = await Api.MintSkin(Address);
                        if (mintedSkin)
                        {

                            MemoryCache.UseCoolerSkin = true;
                            ShowBonusSkinMessage("You now have a cooler skin and can use it!");
                        }
                        else
                        {
                            Debug.Log($"Something went wrong while minting skin");
                            ShowBonusSkinMessage("Something went wrong :(");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Something went wrong while burning tokens {ex.Message}");
                    ShowBonusSkinMessage("Something went wrong :(");
                }
            }
            else 
            {
                ShowBonusSkinMessage("No skin to burn");
            }
        }
    
        private void ShowBonusSkinMessage(string message)
        {
            m_BonusSkinMessage.text = message;
            m_BonusSkinMessage.gameObject.SetActive(true);
            m_BonusSkinGetButton.gameObject.SetActive(false);
            m_BonusSkinLoading.gameObject.SetActive(false);
        }
    }
}
