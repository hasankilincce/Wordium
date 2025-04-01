using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;

public class FirebaseAuthManager : MonoBehaviour
{
    public TasksManager tasksManager;
    public MainMenuUI mainMenuUI;
    public UserDataManager userDataManager;
    public SettingsUI settingsUI;
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;

    void Start()
    {
        FirebaseInitializerHelper.InitializeIfNeeded().ContinueWithOnMainThread(task =>
        {
            if(task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    user = auth.CurrentUser;
                    Debug.Log("Mevcut kullanıcı bulundu: " + user.UserId);
                    SaveUserData(user);
                }
                else
                {
                    SignInAnonymously();
                }
            }
            else
            {
                Debug.LogError("Firebase bağımlılıkları uygun değil: " + task.Result);
            }
        });
    }

    private void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Anonim giriş başarısız: " + task.Exception);
                return;
            }

            user = task.Result.User;
            Debug.Log("Anonim giriş başarılı! Kullanıcı ID: " + user.UserId);
            SaveUserData(user);
        });
    }

    private void SaveUserData(FirebaseUser user)
    {
        if (user == null) return;

        DocumentReference userDocRef = db.Collection("users").Document(user.UserId);

        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "userId", user.UserId },
            { "createdAt", FieldValue.ServerTimestamp },
            { "lastLogin", FieldValue.ServerTimestamp },
            { "isAnonymous", user.IsAnonymous },
            { "level", 1},
            { "taskLevel", 1},
            { "health", 5},
            { "coins", 0},
            { "dictionaryJoker", 1},
            { "diceJoker", 1},
            { "pencilJoker", 1},
            { "brushJoker", 1}
        };

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                Debug.Log("Kullanıcı zaten Firestore'da kayıtlı, giriş zamanı güncellendi.");
                userDocRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "lastLogin", FieldValue.ServerTimestamp }
                });
            }
            else
            {
                Debug.Log("Yeni kullanıcı oluşturuluyor...");
                userDocRef.SetAsync(userData).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsCompleted)
                    {
                        Debug.Log("Kullanıcı Firestore'a kaydedildi. Zaman bilgisi tekrar okunuyor...");
                        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(snapshotTask =>
                        {
                            if (snapshotTask.IsCompleted && snapshotTask.Result.Exists)
                            {
                                Timestamp createdAt = snapshotTask.Result.GetValue<Timestamp>("createdAt");
                                Debug.Log("Gerçek oluşturulma zamanı: " + createdAt.ToDateTime());
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("Kullanıcı Firestore'a kaydedilemedi: " + saveTask.Exception);
                    }
                });
            }
        });
    }

    public void SignOut()
    {
        if (auth != null)
        {
            Debug.Log("Kullanıcı çıkış yapıyor, yerel veriler silinecek...");
            userDataManager.ClearLocalData();
            auth.SignOut();
            Debug.Log("Yeni anonim kullanıcı oluşturuluyor...");
            SignInAnonymously();
            settingsUI.HideDeleteDataPanel();
            mainMenuUI.ReturnMainMenu();

            tasksManager.ResetAllTaskData();

        }
        else
        {
            Debug.LogError("Firebase Auth başlatılmamış.");
        }
    }
}
