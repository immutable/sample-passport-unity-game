using HyperCasual.Core;
using UnityEngine;
using TMPro;
using Immutable.Passport;
using Cysharp.Threading.Tasks;
using HyperCasual.Runner;

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

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_IPHONE
            bool isConnected = MemoryCache.IsConnected;
            if (isConnected)
            {
                await ShowConnectedEmail();
            }
            else
            {
                bool hasCredsSaved = await Passport.Instance.HasCredentialsSaved();
                if (hasCredsSaved)
                {
                    bool connected = await Passport.Instance.ConnectSilent();
                    if (connected)
                    {
                        await ShowConnectedEmail();
                    }
                    else
                    {
                        Debug.Log("Attempted to silently connect to Passport"); 
                    }
                }
            }
#endif
            m_Loading.gameObject.SetActive(false);
            m_StartButton.gameObject.SetActive(true);
        }

        private async UniTask ShowConnectedEmail()
        {
            m_ConnectedAs.gameObject.SetActive(true);
            m_LogoutButton.gameObject.SetActive(true);
            string? email = await Passport.Instance.GetEmail();
            string? address = await Passport.Instance.GetAddress();
            m_ConnectedAs.text = email != null && address != null ? $"{email}\n{address}" : "Connected";
        }

        public async void OnLogout()
        {
            await Passport.Instance.Logout();
            m_ConnectedAs.gameObject.SetActive(false);
            m_LogoutButton.gameObject.SetActive(false);
            SaveManager.Instance.LevelProgress = 0;
            MemoryCache.IsConnected = false;
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