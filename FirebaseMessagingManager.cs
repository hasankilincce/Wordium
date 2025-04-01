using UnityEngine;
using Firebase;
using Firebase.Messaging;
using Firebase.Extensions;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class FirebaseMessagingManager : MonoBehaviour
{
    public static bool IsFirebaseReady { get; private set; }

    public static bool NotificationsEnabled => PlayerPrefs.GetInt("notification", 1) == 1;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeFirebase();
    }

    void OnEnable()
    {
        SettingsUI.OnNotificationSettingsChanged += OnNotificationSettingsChanged;
    }

    void OnDisable()
    {
        SettingsUI.OnNotificationSettingsChanged -= OnNotificationSettingsChanged;
    }

    void InitializeFirebase()
    {
        FirebaseInitializerHelper.InitializeIfNeeded().ContinueWithOnMainThread(task =>
        {
            if(task.Result == DependencyStatus.Available)
            {
                IsFirebaseReady = true;
                Debug.Log("Firebase Hazır.");
                InitializeFirebaseMessaging();
                RequestNotificationPermission();
            }
            else
            {
                Debug.LogError($"Firebase hata: {task.Result}");
            }
        });
    }

    void InitializeFirebaseMessaging()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    void RequestNotificationPermission()
    {
#if UNITY_IOS
        StartCoroutine(RequestIOSNotificationPermission());
#elif UNITY_ANDROID
        if (AndroidNotificationPermissionNeeded())
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
#endif
    }

#if UNITY_IOS
    System.Collections.IEnumerator RequestIOSNotificationPermission()
    {
        var authOptions = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        using (var req = new AuthorizationRequest(authOptions, true))
        {
            while (!req.IsFinished)
                yield return null;

            if (req.Granted)
                Debug.Log("📲 [iOS] Bildirim izni verildi.");
            else
                Debug.LogWarning("📲 [iOS] Bildirim izni reddedildi.");
        }
    }
#endif

#if UNITY_ANDROID
    bool AndroidNotificationPermissionNeeded()
    {
        return AndroidVersion >= 33 && !Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS");
    }

    int AndroidVersion
    {
        get
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
    }
#endif

    void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log($"🔑 Firebase Token alındı: {token.Token}");
    }

    void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        if (NotificationsEnabled)
        {
            Debug.Log($"🔔 Bildirim Geldi: {e.Message.Notification.Title} - {e.Message.Notification.Body}");
            // Bildirimi uygulama içinde gösterme işlemi eklenebilir.
        }
        else
        {
            Debug.Log("Bildirimler kapalı, gelen bildirim gösterilmeyecek.");
        }
    }

    void OnNotificationSettingsChanged(bool enabled)
    {
        Debug.Log($"🔔 Bildirim ayarı değişti: {(enabled ? "Açık" : "Kapalı")}");
        // Ek işlemler yapılabilir.
    }
}
