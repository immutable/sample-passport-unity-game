using HyperCasual.Core;
using UnityEngine;
using TMPro;
using Immutable.Passport;

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

        public override void Show() 
        {
            Debug.Log("Showing Main menu screen");
            base.Show();

            if (Passport.Instance.HasCredentialsSaved())
            {
                m_ConnectedAs.gameObject.SetActive(true);
                m_LogoutButton.gameObject.SetActive(true);
                string? email = Passport.Instance.GetEmail();
                m_ConnectedAs.text = email != null ? email : "Connected";
            }
        }

        public void OnLogout()
        {
            Passport.Instance.Logout();
            m_ConnectedAs.gameObject.SetActive(false);
            m_LogoutButton.gameObject.SetActive(false);
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