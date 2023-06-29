using UnityEngine;
using HyperCasual.Core;
using Immutable.Passport;
using HyperCasual.Runner;

namespace HyperCasual.Gameplay
{
    /// <summary>
    /// This View contains the splash screen functionalities
    /// </summary>
    public class SplashScreen : View
    {
        public async override void Show() 
        {
            base.Show();
            Debug.Log("Init splash screen");
            await Passport.Init();
            Debug.Log("Passport done");
            UIManager.Instance.Show<MainMenu>();
            AudioManager.Instance.PlayMusic(SoundID.MenuMusic);
        }
    }
}