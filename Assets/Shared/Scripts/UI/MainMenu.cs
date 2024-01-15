using System.Collections.Generic;
using HyperCasual.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Immutable.Passport;
using Immutable.Passport.Model;
using Cysharp.Threading.Tasks;

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
        HyperCasualButton m_InventoryButton;
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
        [SerializeField]
        Toggle m_zkEVMToggle;

        public override async void Show() 
        {
            Debug.Log("Showing Main menu screen");
            base.Show();

            m_Loading.gameObject.SetActive(true);
            m_ShopButton.gameObject.SetActive(false);
            m_InventoryButton.gameObject.SetActive(false);
            bool isConnected = MemoryCache.IsConnected;
            if (isConnected)
            {
                await ShowConnectedEmail();
            }
            else
            {
                m_ShopButton.gameObject.SetActive(false);
                m_InventoryButton.gameObject.SetActive(false);
                bool hasCredsSaved = await Passport.Instance.HasCredentialsSaved();
                if (hasCredsSaved)
                {
                    bool connected = await Passport.Instance.ConnectImx(useCachedSession: true);
                    if (connected)
                    {
                        MemoryCache.IsConnected = true;
                        await ShowConnectedEmail();
                    }
                    else
                    {
                        Debug.Log("Attempted to silently connect to Passport, but couldn't so logged out");
                        ResetValues();
                        hasCredsSaved = await Passport.Instance.HasCredentialsSaved();
                        Debug.Log($"After logged out is credentials still there? {hasCredsSaved}");
                    }
                }
            }

            m_Loading.gameObject.SetActive(false);
            m_StartButton.gameObject.SetActive(true);

            m_zkEVMToggle.isOn = SaveManager.Instance.ZkEvm;
            m_zkEVMToggle.onValueChanged.AddListener(delegate {
                SaveManager.Instance.ZkEvm = m_zkEVMToggle.isOn;
            });
        }

        private async UniTask ShowConnectedEmail()
        {
            m_ConnectedAs.gameObject.SetActive(true);
            m_LogoutButton.gameObject.SetActive(true);
            
            string? email = await Passport.Instance.GetEmail();
            string? address = null;
            if (SaveManager.Instance.ZkEvm)
            {
                await Passport.Instance.ConnectEvm();
                List<string> accounts = await Passport.Instance.ZkEvmRequestAccounts();
                address = accounts[0];
            } else {
                address = await Passport.Instance.GetAddress();
            }
            m_ConnectedAs.text = email != null && address != null ? $"{email}\n{address}" : "Connected";
            // m_ShopButton.gameObject.SetActive(true);
            // m_InventoryButton.gameObject.SetActive(true);
        }

        public async void OnLogout()
        {
            m_LogoutButton.gameObject.SetActive(false);
            m_ConnectedAs.gameObject.SetActive(false);
            m_Loading.gameObject.SetActive(true);
            m_ShopButton.gameObject.SetActive(false);
            m_InventoryButton.gameObject.SetActive(false);
            m_StartButton.gameObject.SetActive(false);

#if UNITY_ANDROID || UNITY_IPHONE || (UNITY_STANDALONE_OSX && !UNITY_EDITOR_OSX)
            await Passport.Instance.LogoutPKCE();
#else
            await Passport.Instance.Logout();
#endif
            m_Loading.gameObject.SetActive(false);
            m_StartButton.gameObject.SetActive(true);
            ResetValues();
        }

        private void ResetValues()
        {
            SaveManager.Instance.LevelProgress = 0;
            MemoryCache.IsConnected = false;
            MemoryCache.UseNewSkin = false;
            MemoryCache.UseCoolerSkin = false;
        }

        void OnEnable()
        {
            m_StartButton.AddListener(OnStartButtonClick);
            m_InventoryButton.AddListener(OnInventoryButtonClick);
            m_ShopButton.AddListener(OnShopButtonClick);
        }
        
        void OnDisable()
        {
            m_StartButton.RemoveListener(OnStartButtonClick);
            m_InventoryButton.RemoveListener(OnInventoryButtonClick);
            m_ShopButton.RemoveListener(OnShopButtonClick);
        }

        void OnStartButtonClick()
        {
            m_StartButtonEvent.Raise();
            AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }

        void OnInventoryButtonClick()
        {
            // Not implemented
            // UIManager.Instance.Show<InventoryView>();
            // AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }

        void OnShopButtonClick()
        {
            // Not implemented
            // UIManager.Instance.Show<MarketplaceView>();
            // AudioManager.Instance.PlayEffect(SoundID.ButtonSound);
        }
    }
}