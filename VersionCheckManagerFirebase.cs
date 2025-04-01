using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;

public class VersionCheckManagerFirebase : MonoBehaviour
{
    public PopUpUI popUpUI;

    // Uygulamadaki yerel sürüm kodu
    private int localVersionCode = 10702;


    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase hazır. Remote Config başlatılıyor.");
                InitializeRemoteConfig();
            }
            else
            {
                Debug.LogError("Firebase bağımlılıkları çözülürken hata: " + task.Result);
            }
        });
    }

    void InitializeRemoteConfig()
    {
        // Sadece yeni bir ConfigSettings örneği oluşturun, ekstra ayar yapmaya çalışmayın.
        var configSettings = new ConfigSettings();
        FirebaseRemoteConfig.DefaultInstance.SetConfigSettingsAsync(configSettings).ContinueWithOnMainThread(task =>
        {
            // Varsayılan değerleri belirleyin.
            var defaults = new Dictionary<string, object>
            {
                { "min_app_version", localVersionCode }
            };
            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task2 =>
            {
                // FetchAsync çağrısına minimum fetch süresini TimeSpan parametresi olarak geçin.
                FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.FromSeconds(0)).ContinueWithOnMainThread(fetchTask =>
                {
                    if (fetchTask.IsCompleted)
                    {
                        FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                        {
                            int minVersion = (int)FirebaseRemoteConfig.DefaultInstance.GetValue("min_app_version").LongValue;
                            Debug.Log("Sunucudan gelen min_app_version: " + minVersion);

                            if (localVersionCode < minVersion)
                            {
                                popUpUI.ShowUpgradeAppPanel();
                                Debug.Log("Uygulama güncellenmeli. Mevcut sürüm: " + localVersionCode + ", Gerekli sürüm: " + minVersion);
                            }
                            else
                            {
                                Debug.Log("Uygulama güncel.");
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("Remote Config verileri çekilemedi: " + fetchTask.Exception);
                    }
                });
            });
        });
    }

    // Güncelleme butonuna bağlı metod
    public void OnClick_UpdateButton()
    {
        #if UNITY_ANDROID
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.oassis.wordiumgame");
        #elif UNITY_IOS
            Application.OpenURL("https://apps.apple.com/us/app/wordium/id6742060890");
        #else
            Debug.Log("Platform güncelleme linki desteklenmiyor.");
        #endif
    }


    // İsteğe bağlı: Çıkış butonu
    public void OnClick_ExitApp()
    {
        Application.Quit();
    }
}
