using HyperCasual.Core;
using UnityEngine;
using TMPro;
using Immutable.Passport;
using System;

namespace HyperCasual.Runner
{
    /// <summary>
    /// This View contains main menu functionalities
    /// </summary>
    public class MainMenu : View
    {
        [SerializeField]
        HyperCasualButton m_StartButton;
        [SerializeField]
        HyperCasualButton m_SettingsButton;
        [SerializeField]
        HyperCasualButton m_ShopButton;
        [SerializeField]
        AbstractGameEvent m_StartButtonEvent;
        [SerializeField]
        TextMeshProUGUI m_ConnectedAs;
        [SerializeField]
        HyperCasualButton m_LogoutButton;
        [SerializeField]
        GameObject m_Loading;

        public override async void Show() 
        {
            Debug.Log("Showing Main menu screen");
            base.Show();

            try
            {
                if (Passport.Instance.HasCredentialsSaved())
                {
                    await Passport.Instance.ConnectSilent();
                    m_ConnectedAs.gameObject.SetActive(true);
                    m_LogoutButton.gameObject.SetActive(true);
                    string? email = Passport.Instance.GetEmail();
                    m_ConnectedAs.text = email != null ? email : "Connected";
                }
            }
            catch (Exception)
            {
                Debug.Log("Attempted to silently connect to Passport");
            }
            m_Loading.gameObject.SetActive(false);
            m_StartButton.gameObject.SetActive(true);
        }

        public void OnLogout()
        {
            Passport.Instance.Logout();
            m_ConnectedAs.gameObject.SetActive(false);
            m_LogoutButton.gameObject.SetActive(false);
            SaveManager.Instance.LevelProgress = 0;
        }

        void OnEnable()
        {
            m_StartButton.AddListener(OnStartButtonClick);
            m_SettingsButton.AddListener(OnSettingsButtonClick);
            m_ShopButton.AddListener(OnShopButtonClick);
        }
        
        void OnDisable()
        {
            m_StartButton.RemoveListener(OnStartButtonClick);
            m_SettingsButton.RemoveListener(OnSettingsButtonClick);
            m_ShopButton.RemoveListener(OnShopButtonClick);
        }

        void OnStartButtonClick()
        {
            m_StartButtonEvent.Raise();
            AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }

        void OnSettingsButtonClick()
        {
            UIManager.Instance.Show<SettingsMenu>();
            AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }

        void OnShopButtonClick()
        {
            UIManager.Instance.Show<ShopView>();
            AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }
    }
}