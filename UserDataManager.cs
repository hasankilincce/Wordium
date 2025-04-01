using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;

public class UserDataManager : MonoBehaviour
{
    public MainMenuUI mainMenuUI;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private FirebaseUser user;

    private string localFilePath;
    private UserData currentUserData;
    private UserData lastSyncedUserData;         // Firestore'dan çekilen en son "onaylı" veri

    private bool offlineDataNeedsSync = false;   // Offline veri değişti, internet gelince Firestore'a yazılmalı

    // Burada "updatedAt" alanı ekleyerek, hem yerelde hem Firestore'da en son ne zaman güncellendiğini karşılaştırıyoruz
    [FirestoreData]
    public class UserData
    {
        [FirestoreProperty] public int coins { get; set; }
        [FirestoreProperty] public int health { get; set; }
        [FirestoreProperty] public int level { get; set; }
        [FirestoreProperty] public int taskLevel { get; set; }
        [FirestoreProperty] public int strikeCount { get; set; }
        [FirestoreProperty] public int dictionaryJoker { get; set; }
        [FirestoreProperty] public int diceJoker { get; set; }
        [FirestoreProperty] public int pencilJoker { get; set; }
        [FirestoreProperty] public int brushJoker { get; set; }
        [FirestoreProperty] public Firebase.Firestore.Timestamp createdAt { get; set; }
        [FirestoreProperty] public Firebase.Firestore.Timestamp lastLogin { get; set; }
        [FirestoreProperty] public Firebase.Firestore.Timestamp updatedAt { get; set; }
    }

    private async void Start()
    {
        // Firebase initialization'ını merkezi helper üzerinden bekleyelim
        DependencyStatus dependencyStatus = await FirebaseInitializerHelper.InitializeIfNeeded();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Firebase Dependency hatası: " + dependencyStatus);
            return;
        }

        // Bağımlılık kontrolü tamamlandıktan sonra diğer Firebase işlemlerini gerçekleştirelim
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        localFilePath = Path.Combine(Application.persistentDataPath, "userData.json");

        // 1) Yerel veriyi yükle (PlayerPrefs veya .json). Yoksa varsayılan oluştur.
        LoadLocalData();

        // 2) Eğer kullanıcı yoksa anonim giriş yapalım
        if (user == null)
        {
            SignInAnonymously();
        }
        else
        {
            // 3) İnternet varsa Firestore'dan veri çekelim
            if (IsInternetAvailable())
            {
                FetchFromFirestore();
            }
        }
    }

    private void Update()
    {
        // Internet varsa ve offline'da güncellenen veri varsa, senkronize et
        if (offlineDataNeedsSync && IsInternetAvailable())
        {
            Debug.Log("İnternet geldi, offline'da güncellenen veriler Firestore'a senkronlanacak.");
            SyncLocalToFirestore();
        }
    }

    #region Firebase Auth
    private void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Anonim giriş başarısız: " + task.Exception);
                return;
            }

            user = task.Result.User;
            Debug.Log("Anonim giriş başarılı. UID: " + user.UserId);

            // İlk girişten sonra internet varsa Firestore'dan veri çek
            if (IsInternetAvailable())
            {
                FetchFromFirestore();
            }
        });
    }
    #endregion

    #region Firestore Fetch / Sync
    private async void FetchFromFirestore()
    {
        if (user == null) return;

        DocumentReference userDocRef = db.Collection("users").Document(user.UserId);

        try
        {
            DocumentSnapshot snapshot = await userDocRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Firestore'daki veriyi UserData tipine çevir
                var firestoreData = snapshot.ConvertTo<UserData>();

                // Firestore'dan gelen createdAt bilgisini kaybetmemek için kontrol ediyoruz
                if (firestoreData.createdAt != null)
                {
                    currentUserData.createdAt = firestoreData.createdAt;
                }

                lastSyncedUserData = CloneUserData(firestoreData);

                bool localIsNewer = IsLocalDataNewer(currentUserData, firestoreData);

                if (localIsNewer)
                {
                    Debug.Log("Local veri Firestore'dakinden daha yeni bulundu. Firestore'a yazılıyor...");
                    SyncLocalToFirestore();
                }
                else
                {
                    currentUserData = CloneUserData(firestoreData);
                    Debug.Log("Firestore verisi local veriden daha güncel. Local'i Firestore verisiyle güncelledik.");
                    SaveLocalData();
                }
            }
            else
            {
                Debug.LogWarning("Firestore'da bu kullanıcı için veri yok. Local veriyi Firestore'a ilk kez yazıyoruz...");
                currentUserData.createdAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow); // İlk kez oluşturuluyorsa
                await userDocRef.SetAsync(currentUserData);
                lastSyncedUserData = CloneUserData(currentUserData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Firestore'dan veri çekilirken hata: " + e);
        }
    }

    private async void SyncLocalToFirestore()
    {
        if (!IsInternetAvailable() || user == null) return;

        DocumentReference userDocRef = db.Collection("users").Document(user.UserId);

        // updatedAt güncelleniyor, createdAt sabit kalmalı
        currentUserData.updatedAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow);

        try
        {
            Dictionary<string, object> userDataToUpdate = new Dictionary<string, object>
            {
                { "coins", currentUserData.coins },
                { "health", currentUserData.health },
                { "level", currentUserData.level },
                { "taskLevel", currentUserData.taskLevel },
                { "strikeCount", currentUserData.strikeCount },
                { "dictionaryJoker", currentUserData.dictionaryJoker },
                { "diceJoker", currentUserData.diceJoker },
                { "pencilJoker", currentUserData.pencilJoker },
                { "brushJoker", currentUserData.brushJoker },
                { "updatedAt", currentUserData.updatedAt }
            };

            await userDocRef.UpdateAsync(userDataToUpdate);
            Debug.Log("Local veriler Firestore'a yüklendi, ancak createdAt değiştirilmedi.");

            lastSyncedUserData = CloneUserData(currentUserData);
            offlineDataNeedsSync = false;
        }
        catch (Exception e)
        {
            Debug.LogError("Local veriyi Firestore'a yüklerken hata: " + e);
        }
    }
    #endregion

    #region Local Data (Load/Save)
    private void LoadLocalData()
    {
        if (PlayerPrefs.HasKey("coins"))
        {
            currentUserData = new UserData
            {
                coins           = PlayerPrefs.GetInt("coins"),
                health          = PlayerPrefs.GetInt("health"),
                level           = PlayerPrefs.GetInt("level"),
                taskLevel       = PlayerPrefs.GetInt("taskLevel"),
                strikeCount     = PlayerPrefs.GetInt("strikeCount"),
                dictionaryJoker = PlayerPrefs.GetInt("dictionaryJoker"),
                diceJoker       = PlayerPrefs.GetInt("diceJoker"),
                pencilJoker     = PlayerPrefs.GetInt("pencilJoker"),
                brushJoker      = PlayerPrefs.GetInt("brushJoker"),
                updatedAt       = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow)
            };

            Debug.Log("Local veri PlayerPrefs'ten yüklendi.");
        }
        else
        {
            if (File.Exists(GetLocalFilePath()))
            {
                try
                {
                    string jsonData = File.ReadAllText(GetLocalFilePath());
                    currentUserData = JsonUtility.FromJson<UserData>(jsonData);
                    Debug.Log("Local veri .json dosyasından yüklendi: " + jsonData);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Yerel dosyadan okurken hata. Varsayılan değerlerle başlatılacak: " + e);
                    currentUserData = GetDefaultUserData();
                }
            }
            else
            {
                Debug.Log("Hiç yerel kayıt yok. Varsayılan değer oluşturulacak.");
                currentUserData = GetDefaultUserData();
            }
        }

        SaveLocalData();

        if (mainMenuUI != null)
        {
            mainMenuUI.MainMenuStart();
        }
    }

    private void SaveLocalData()
    {
        if (currentUserData == null)
        {
            Debug.LogWarning("SaveLocalData() çağrıldı ama currentUserData boş.");
            return;
        }

        currentUserData.updatedAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow);

        PlayerPrefs.SetInt("coins", currentUserData.coins);
        PlayerPrefs.SetInt("health", currentUserData.health);
        PlayerPrefs.SetInt("level", currentUserData.level);
        PlayerPrefs.SetInt("strikeCount", currentUserData.strikeCount);
        PlayerPrefs.SetInt("taskLevel", currentUserData.taskLevel);
        PlayerPrefs.SetInt("dictionaryJoker", currentUserData.dictionaryJoker);
        PlayerPrefs.SetInt("diceJoker", currentUserData.diceJoker);
        PlayerPrefs.SetInt("pencilJoker", currentUserData.pencilJoker);
        PlayerPrefs.SetInt("brushJoker", currentUserData.brushJoker);
        PlayerPrefs.Save();

        try
        {
            string jsonData = JsonUtility.ToJson(currentUserData, true);
            File.WriteAllText(GetLocalFilePath(), jsonData);
            Debug.Log("currentUserData .json dosyasına kaydedildi: " + jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError("Veri yerel dosyaya kaydedilemedi: " + e);
        }
    }

    private UserData GetDefaultUserData()
    {
        return new UserData
        {
            level = 1,
            taskLevel = 1,
            strikeCount= 0,
            health = 5,
            coins = 0,
            dictionaryJoker = 10,
            diceJoker = 1,
            pencilJoker = 10,
            brushJoker = 1,
            createdAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow),
            updatedAt = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow)
        };
    }

    private string GetLocalFilePath()
    {
        if (string.IsNullOrEmpty(localFilePath))
        {
            localFilePath = Path.Combine(Application.persistentDataPath, "userData.json");
        }
        return localFilePath;
    }
    #endregion

    #region Public Methods (Update Data, Get Data)
    public void UpdateUserData(int newCoins, int newHealth, int newLevel, int newTaskLevel, int newStrikeCount,
                               int newDictionaryJoker, int newDiceJoker,
                               int newPencilJoker, int newBrushJoker)
    {
        if (currentUserData == null)
        {
            currentUserData = GetDefaultUserData();
        }

        currentUserData.coins           = newCoins;
        currentUserData.health          = newHealth;
        currentUserData.level           = newLevel;
        currentUserData.taskLevel       = newTaskLevel;
        currentUserData.strikeCount     = newStrikeCount;
        currentUserData.dictionaryJoker = newDictionaryJoker;
        currentUserData.diceJoker       = newDiceJoker;
        currentUserData.pencilJoker     = newPencilJoker;
        currentUserData.brushJoker      = newBrushJoker;

        currentUserData.updatedAt       = Firebase.Firestore.Timestamp.FromDateTime(DateTime.UtcNow);

        SaveLocalData();

        if (IsInternetAvailable())
        {
            SyncLocalToFirestore();
        }
        else
        {
            offlineDataNeedsSync = true;
        }
    }

    public UserData GetUserData()
    {
        return currentUserData;
    }
    #endregion

    #region Helpers
    private bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    private bool IsLocalDataNewer(UserData local, UserData remote)
    {
        if (remote == null) return true;
        if (local == null) return false;

        DateTime localTime  = local.updatedAt.ToDateTime();
        DateTime remoteTime = remote.updatedAt.ToDateTime();

        return localTime > remoteTime;
    }

    private UserData CloneUserData(UserData original)
    {
        if (original == null) return null;

        return new UserData
        {
            coins           = original.coins,
            health          = original.health,
            level           = original.level,
            taskLevel       = original.taskLevel,
            strikeCount     = original.strikeCount,
            dictionaryJoker = original.dictionaryJoker,
            diceJoker       = original.diceJoker,
            pencilJoker     = original.pencilJoker,
            brushJoker      = original.brushJoker,
            createdAt       = original.createdAt,
            lastLogin       = original.lastLogin,
            updatedAt       = original.updatedAt
        };
    }
    #endregion

    public void ClearLocalData()
    {
        Debug.Log("Yerel veriler varsayılan değerlere sıfırlanıyor...");

        currentUserData = GetDefaultUserData();
        lastSyncedUserData = null;

        PlayerPrefs.SetInt("coins", currentUserData.coins);
        PlayerPrefs.SetInt("health", currentUserData.health);
        PlayerPrefs.SetInt("level", currentUserData.level);
        PlayerPrefs.SetInt("taskLevel", currentUserData.taskLevel);
        PlayerPrefs.SetInt("strikeCount", currentUserData.strikeCount);
        PlayerPrefs.SetInt("dictionaryJoker", currentUserData.dictionaryJoker);
        PlayerPrefs.SetInt("diceJoker", currentUserData.diceJoker);
        PlayerPrefs.SetInt("pencilJoker", currentUserData.pencilJoker);
        PlayerPrefs.SetInt("brushJoker", currentUserData.brushJoker);
        PlayerPrefs.Save();

        SaveLocalData();

        Debug.Log("Yerel veriler varsayılan değerlere sıfırlandı.");
    }

    public void SaveUserData()
    {
        UpdateUserData(
            PlayerPrefs.GetInt("coins"),
            PlayerPrefs.GetInt("health"),
            PlayerPrefs.GetInt("level"),
            PlayerPrefs.GetInt("taskLevel"),
            PlayerPrefs.GetInt("strikeCount"),
            PlayerPrefs.GetInt("dictionaryJoker"),
            PlayerPrefs.GetInt("diceJoker"),
            PlayerPrefs.GetInt("pencilJoker"),
            PlayerPrefs.GetInt("brushJoker")
        );
    }
}
