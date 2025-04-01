using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public GameManager gameManager;
    public PopUpUI popUpUI;


    // Ödüllü reklam (Rewarded Ad)
    private RewardedAd rewardedAd;
    [Header("Rewarded Ad Unit IDs")]
    public string androidRewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
    public string iosRewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";

    // Geçiş reklamı (Interstitial Ad)
    private InterstitialAd interstitialAd;
    [Header("Interstitial Ad Unit IDs")]
    public string androidInterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    public string iosInterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";

    private void Start()
    {
        // AdMob'u başlat (sadece 1 kere yapılmalı)
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob initialized");
            LoadRewardedAd();
            LoadInterstitialAd();
        });
    }

    #region Rewarded Ad İşlemleri

    public void LoadRewardedAd()
    {
        string adUnitId = "";
#if UNITY_ANDROID
        adUnitId = androidRewardedAdUnitId;
#elif UNITY_IOS
        adUnitId = iosRewardedAdUnitId;
#else
        adUnitId = "unexpected_platform";
#endif

        AdRequest request = new AdRequest();

        // Yeni API: RewardedAd.Load() ile reklam yükleniyor.
        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Failed to load rewarded ad: " + error);
                return;
            }
            rewardedAd = ad;

            // Reklamın kapandığında veya gösterim sırasında hata olursa yeniden yükleme yapıyoruz.
            rewardedAd.OnAdFullScreenContentClosed += HandleRewardedAdClosed;
            rewardedAd.OnAdFullScreenContentFailed += HandleRewardedAdFailedToShow;

            Debug.Log("Rewarded ad loaded.");
        });
    }

    /// <summary>
    /// Reklamı göster ve eğer ödül kazanılırsa oyuncuya 'amount' kadar hamle ekle.
    /// </summary>
    public void ShowRewardedAd(int amount, bool isExtraMove)
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show((Reward reward) =>
            {
                if (isExtraMove)
                {
                    gameManager.moveCountUpdate(amount);
                    Debug.Log("Rewarded ad completed. Player moves increased to: " + amount);

                    popUpUI.CloseExtraMovePanel();
                }
                else
                {
                    PlayerPrefs.SetInt("health", PlayerPrefs.GetInt("health") + amount);

                    popUpUI.CloseWaitHealthPanel();
                }
                
            });
        }
        else
        {
            Debug.LogWarning("Rewarded ad is not ready.");
        }
    }

    private void HandleRewardedAdClosed()
    {
        Debug.Log("Rewarded ad closed. Reloading rewarded ad...");
        LoadRewardedAd();
    }

    private void HandleRewardedAdFailedToShow(AdError error)
    {
        Debug.LogError("Rewarded ad failed to show: " + error);
        LoadRewardedAd();
    }

    #endregion

    #region Interstitial Ad İşlemleri

    public void LoadInterstitialAd()
    {
        string adUnitId = "";
#if UNITY_ANDROID
        adUnitId = androidInterstitialAdUnitId;
#elif UNITY_IOS
        adUnitId = iosInterstitialAdUnitId;
#else
        adUnitId = "unexpected_platform";
#endif

        AdRequest request = new AdRequest();

        // Yeni API: InterstitialAd.Load() ile geçiş reklamı yükleniyor.
        InterstitialAd.Load(adUnitId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Failed to load interstitial ad: " + error);
                return;
            }
            interstitialAd = ad;

            // Reklam kapandığında yeniden yükleyelim.
            interstitialAd.OnAdFullScreenContentClosed += HandleInterstitialAdClosed;
            interstitialAd.OnAdFullScreenContentFailed += HandleInterstitialAdFailedToShow;

            Debug.Log("Interstitial ad loaded.");
        });
    }

    /// <summary>
    /// Geçiş reklamını göster.
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning("Interstitial ad is not ready.");
        }
    }

    private void HandleInterstitialAdClosed()
    {
        Debug.Log("Interstitial ad closed. Reloading interstitial ad...");
        LoadInterstitialAd();
    }

    private void HandleInterstitialAdFailedToShow(AdError error)
    {
        Debug.LogError("Interstitial ad failed to show: " + error);
        LoadInterstitialAd();
    }


   public void ShowInterstitialAdWithRoutine()
    {
        int adCounter = PlayerPrefs.GetInt("interstitialAdCount");

        if (adCounter >= 3)
        {
            ShowInterstitialAd();
            PlayerPrefs.SetInt("interstitialAdCount", 0);
        }
        else
        {
            PlayerPrefs.SetInt("interstitialAdCount", adCounter + 1);
        }

        PlayerPrefs.Save();
    }


    #endregion


}
