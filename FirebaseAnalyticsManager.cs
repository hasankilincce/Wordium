using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;

public class FirebaseAnalyticsManager : MonoBehaviour
{
    void Start()
    {
        FirebaseInitializerHelper.InitializeIfNeeded().ContinueWithOnMainThread(task =>
        {
            if(task.Result == DependencyStatus.Available)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                Debug.Log("Firebase Analytics hazır.");
            }
            else
            {
                Debug.LogError("Firebase bağımlılıkları uygun değil: " + task.Result);
            }
        });
    }

    public void LogUserLogin(string userId)
    {
        FirebaseAnalytics.SetUserId(userId);
        FirebaseAnalytics.LogEvent("user_login");
        Debug.Log("Firebase Analytics: Kullanıcı giriş yaptı! User ID: " + userId);
    }

    public void LogLevelComplete(int levelNumber)
    {
        FirebaseAnalytics.LogEvent("level_complete",
            new Parameter("level_number", levelNumber)
        );
        Debug.Log("Seviye tamamlandı: " + levelNumber);
    }

    public void LogLevelFail(int levelNumber)
    {
        FirebaseAnalytics.LogEvent("level_fail",
            new Parameter("level_number", levelNumber)
        );
        Debug.Log("Seviye başarısız: " + levelNumber);
    }

    public void LogLevelStart(int levelNumber)
    {
        FirebaseAnalytics.LogEvent("level_start",
            new Parameter("level_number", levelNumber)
        );
        Debug.Log("Seviye başladı: " + levelNumber);
    }
}
