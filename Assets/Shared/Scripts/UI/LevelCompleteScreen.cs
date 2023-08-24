using HyperCasual.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Immutable.Passport;
using Immutable.Passport.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
        GameObject m_SkinContainer;

        [SerializeField]
        TextMeshProUGUI m_MintedTitle;
        [SerializeField]
        HyperCasualButton m_MintedNextButton;

        [SerializeField]
        TextMeshProUGUI m_SkinMessage;
        [SerializeField]
        HyperCasualButton m_SkinUseButton;
        [SerializeField]
        HyperCasualButton m_SkinNextButton;
        [SerializeField]
        GameObject m_SkinLoading;
        
        /// <summary>
        /// The slider that displays the XP value 
        /// </summary>
        public Slider XpSlider => m_XpSlider;

        int m_GoldValue;

        private ConnectResponse? connectResponse;

        private ApiService api = new();
        private string? address;
        
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
            ShowSkinContainer(false);

            // Continue with Passport button
            m_ContinuePassportButton.RemoveListener(OnContinueButtonClicked);
            m_ContinuePassportButton.AddListener(OnContinueButtonClicked);

            // Burn to use new skin button
            m_SkinUseButton.RemoveListener(OnBurnTokens);
            m_SkinUseButton.AddListener(OnBurnTokens);

            // Next level button on Completed screen
            m_NextButton.RemoveListener(OnNextButtonClicked);
            // Show new skin when level 2 completes
            // And user is not already using it
            if (MemoryCache.CurrentLevel == 2 && !MemoryCache.UseNewSkin)
            {
                m_NextButton.AddListener(OnNextButtonClickedShowNewSkin);
            }
            else
            {
                m_NextButton.AddListener(OnNextButtonClicked);
            }

            // Next level button on Minted Fox (and Tokens) screen
            m_MintedNextButton.RemoveListener(OnNextButtonClicked);
            m_MintedNextButton.AddListener(OnNextButtonClicked);

            // Next level button on New Skin Unlocked screen
            m_SkinNextButton.RemoveListener(OnNextButtonClicked);
            m_SkinNextButton.AddListener(OnNextButtonClicked);

            // If the user is connected to Passport, try and mint the tokens in the background
            // if minting did not complete in time, we just ignore any errors
            if (connected)
            {
                MintTokens();
            }
        }

        void OnNextButtonClicked()
        {
            Debug.Log("On Next Button Clicked");
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
                // Get address and save it
                address = await Passport.Instance.GetAddress();

                // Successfully connected                
                MintFoxAndTokens();
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
            ShowSkinContainer(true);
            ShowCompletedContainer(false);
        }

        void ShowCompletedContainer(bool show)
        {
            m_CompletedContainer.gameObject.SetActive(show);
        }

        void ShowMintedContainer(bool show)
        {
            m_MintedContainer.gameObject.SetActive(show);
        }

        void ShowSkinContainer(bool show)
        {
            m_SkinContainer.gameObject.SetActive(show);
            m_SkinMessage.gameObject.SetActive(false);
            m_SkinUseButton.gameObject.SetActive(true);
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
            if (connectResponse != null)
            {
                Application.OpenURL(connectResponse.url);
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

        public void OnWalletClicked()
        {
            if (address != null)
            {
                Application.OpenURL($"http://localhost:6060/wallet?user={address}");
            }
        }

        private async void MintFoxAndTokens()
        {
            // Mint fox
            bool mintedFox = await api.MintFox(address);

            if (StarCount > 0)
            {
                // User collected tokens
                bool mintedTokens = await api.MintTokens(StarCount, address);

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
            try
            {
                address ??= await Passport.Instance.GetAddress();
                if (StarCount > 0)
                {
                    bool mintedTokens = await api.MintTokens(StarCount, address);
                    Debug.Log(mintedTokens ? "Minted tokens" : $"Failed to mint tokens");
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error minting tokens: {ex.Message}");
            }
        }

        private async void OnBurnTokens()
        {
            Debug.Log("On Burn Tokens...");
            m_SkinLoading.gameObject.SetActive(true);
            m_SkinUseButton.gameObject.SetActive(false);

            address ??= await Passport.Instance.GetAddress();
            List<TokenModel> tokens = await api.GetTokens(3, address);
            if (tokens.Count == 3)
            {
                try
                {
                    List<NftTransferDetails> transferDetails = new List<NftTransferDetails>();
                    tokens.ForEach(delegate (TokenModel token)
                    {
                        Debug.Log($"Got token ID: {token.token_id}");
                        transferDetails.Add(new NftTransferDetails(
                            "0x0000000000000000000000000000000000000000".ToLower(),
                            token.token_id,
                            ApiService.TOKEN_TOKEN_ADDRESS.ToLower()));
                    });
                    var response = await Passport.Instance.ImxBatchNftTransfer(transferDetails.ToArray());

                    m_SkinMessage.text = "You now have the skin and can use it!";
                    m_SkinMessage.gameObject.SetActive(true);
                    m_SkinUseButton.gameObject.SetActive(false);
                    MemoryCache.UseNewSkin = true;
                }
                catch (Exception ex)
                {
                    Debug.Log($"Something went wrong while burning tokens {ex.Message}");
                    m_SkinMessage.text = "Something went wrong :(";
                    m_SkinMessage.gameObject.SetActive(true);
                    m_SkinUseButton.gameObject.SetActive(false);
                }
            }
            else 
            {
                m_SkinMessage.text = "Not enough tokens to burn";
                m_SkinMessage.gameObject.SetActive(true);
                m_SkinUseButton.gameObject.SetActive(false);
            }

            m_SkinLoading.gameObject.SetActive(false);
        }
    }
}
