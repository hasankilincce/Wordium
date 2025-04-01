using UnityEngine;
using Firebase;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirestoreDataCache : MonoBehaviour
{
    private FirebaseFirestore db;

    [NonSerialized]
    public FirestoreTaskDocumentListWrapper loadedData;

    [Serializable]
    public class FirestoreTaskDocument
    {
        public int taskLevel;
        public List<string> tasks;
    }

    [Serializable]
    public class FirestoreTaskDocumentListWrapper
    {
        public List<FirestoreTaskDocument> items;
    }

    private async void Start()
    {
        var dependencyStatus = await FirebaseInitializerHelper.InitializeIfNeeded();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase başlatılamadı: {dependencyStatus}.");
            loadedData = null;
            return;
        }

        db = FirebaseFirestore.DefaultInstance;
        FetchAndStoreData();
    }

    private async void FetchAndStoreData()
    {
        try
        {
            var snapshot = await db.Collection("tasks").GetSnapshotAsync();
            List<FirestoreTaskDocument> docList = new List<FirestoreTaskDocument>();

            foreach (DocumentSnapshot docSnap in snapshot.Documents)
            {
                if (!docSnap.Exists) 
                    continue;

                int levelVal = docSnap.ContainsField("taskLevel")
                    ? docSnap.GetValue<int>("taskLevel")
                    : 0;
                List<string> tasksArray = docSnap.ContainsField("tasks")
                    ? new List<string>(docSnap.GetValue<List<string>>("tasks"))
                    : new List<string>();

                var taskDoc = new FirestoreTaskDocument
                {
                    taskLevel = levelVal,
                    tasks     = tasksArray
                };
                docList.Add(taskDoc);
            }

            loadedData = new FirestoreTaskDocumentListWrapper { items = docList };
            Debug.Log($"✔️ Firestore'dan {docList.Count} doküman çekildi.");

            // Verileri TasksManager’a aktar
            TasksManager tm = FindObjectOfType<TasksManager>();
            if (tm != null)
            {
                tm.SetDataFromFirestore(loadedData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Firestore veri çekme hatası: {ex.Message}");
            loadedData = null;
        }
    }
}
