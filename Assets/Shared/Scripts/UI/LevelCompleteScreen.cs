using HyperCasual.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Immutable.Passport;
using Immutable.Passport.Model;
using System;
using UnityEngine.Networking;
using System.Collections;
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
        HyperCasualButton m_SkinUseButton;
        [SerializeField]
        HyperCasualButton m_SkinNextButton;
        
        /// <summary>
        /// The slider that displays the XP value 
        /// </summary>
        public Slider XpSlider => m_XpSlider;

        int m_GoldValue;

        private ConnectResponse? connectResponse;

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

            m_ContinuePassportButton.RemoveListener(OnContinueButtonClicked);
            m_ContinuePassportButton.AddListener(OnContinueButtonClicked);

            bool hasIceSkin = PlayerPrefsUtils.Read<bool>("hasIceSkin");
            m_NextButton.RemoveListener(OnNextButtonClicked);
            Debug.Log($"LevelCompleteScreen OnEnable Completed Level {SaveManager.Instance.LevelProgress}");
            if (SaveManager.Instance.LevelProgress == 2 && !hasIceSkin)
            {
                m_NextButton.AddListener(OnNextButtonClickedShowNewSkin);
            }
            else
            {
                m_NextButton.AddListener(OnNextButtonClicked);
            }

            m_MintedNextButton.RemoveListener(OnNextButtonClicked);
            m_MintedNextButton.AddListener(OnNextButtonClicked);

            m_SkinNextButton.RemoveListener(OnNextButtonClicked);
            m_SkinNextButton.AddListener(OnNextButtonClicked);
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

                address = await Passport.Instance.GetAddress();

                // Successfully connected

                // Mint fox
                bool minted = await MintFox();
                if (minted)
                {
                    Debug.Log($"Minted a fox");
                    if (StarCount > 0) {
                        m_MintedTitle.text = $"You now own a Fox and {StarCount} tokens!";
                    }
                    else
                    {
                        m_MintedTitle.text = $"You now own a Fox!";
                    }
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
                Application.OpenURL($"https://api.sandbox.x.immutable.com/v1/assets?user={address}");
            }
        }

        async UniTask<bool> MintFox()
        {
            if (address != null)
            {
                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("toUserWallet", address));
                using var client = new HttpClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, "http://localhost:6060/mint/character") { Content = new FormUrlEncodedContent(nvc) };
                using var res = await client.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            return false;
        }
    }
}
