using HyperCasual.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Immutable.Passport;
using Immutable.Passport.Model;
using System;

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
        GameObject m_ConnectBrowserContainer;
        [SerializeField]
        TextMeshProUGUI m_VerificationCode;
        [SerializeField]
        GameObject m_VerifyingCodeProgress;
        
        /// <summary>
        /// The slider that displays the XP value 
        /// </summary>
        public Slider XpSlider => m_XpSlider;

        int m_GoldValue;

        private ConnectResponse? connectResponse;
        
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
            ShowContinueWithPassportButton(!connected);
            ShowNextButton(connected);

            m_ContinuePassportButton.RemoveListener(OnContinueButtonClicked);
            m_ContinuePassportButton.AddListener(OnContinueButtonClicked);

            m_NextButton.RemoveListener(OnNextButtonClicked);
            m_NextButton.AddListener(OnNextButtonClicked);
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
                connectResponse = await Passport.Instance.Connect();
                if (connectResponse != null)
                {
                    // Code confirmation required
                    m_CompletedContainer.gameObject.SetActive(false);

                    m_ConnectBrowserContainer.gameObject.SetActive(true);
                    m_VerificationCode.text = connectResponse.code;
                    
                    await Passport.Instance.ConfirmCode();
                    m_ConnectBrowserContainer.gameObject.SetActive(false);

                    m_CompletedContainer.gameObject.SetActive(true);

                    MemoryCache.IsConnected = true;

                    ShowContinueWithPassportButton(false);
                    HideLoading();
                    ShowNextButton(true);
                }
                else
                {
                    // No need to confirm code, log user straight in
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
    }
}
