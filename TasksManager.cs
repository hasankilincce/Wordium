using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class TasksManager : MonoBehaviour
{
    private static readonly Regex parseRegex = 
        new Regex(@"G(?<g>\d{2})T(?<t>\d+)A(?<a>\d)M(?<m>\d+)", RegexOptions.Compiled);

    private string localFilePath;
    private TaskDocumentListWrapper loadedWrapper;

    public enum TaskField
    {
        Current     = 0,
        Target      = 1,
        Award       = 2,
        Amount      = 3,
        IsActive    = 4,
        IsCompleted = 5
    }

    // 9 görev slotu
    public List<List<int>> taskControllers = new List<List<int>>
    {
        new List<int> { 0, 0, 0, 0, 0, 0}, // G01 -> index=0
        new List<int> { 0, 0, 0, 0, 0, 0}, // G02 -> index=1
        new List<int> { 0, 0, 0, 0, 0, 0}, // G03 -> index=2
        new List<int> { 0, 0, 0, 0, 0, 0}, // G04 -> index=3
        new List<int> { 0, 0, 0, 0, 0, 0}, // G05 -> index=4
        new List<int> { 0, 0, 0, 0, 0, 0}, // G06 -> index=5
        new List<int> { 0, 0, 0, 0, 0, 0}, // G07 -> index=6
        new List<int> { 0, 0, 0, 0, 0, 0}, // G08 -> index=7
        new List<int> { 0, 0, 0, 0, 0, 0}  // G09 -> index=8
    };

    private void Start()
    {
        // JSON’dan da okuyacaksanız
        localFilePath = Path.Combine(Application.persistentDataPath, "tasks.json");
        LoadTasksFromJson();
    }

    private void LoadTasksFromJson()
    {
        if (!File.Exists(localFilePath))
        {
            Debug.LogError($"tasks.json not found: {localFilePath}");
            return;
        }

        string jsonData = File.ReadAllText(localFilePath);
        loadedWrapper = JsonUtility.FromJson<TaskDocumentListWrapper>(jsonData);

        if (loadedWrapper?.items == null)
        {
            Debug.LogWarning("No valid data found in tasks.json or format is incorrect.");
            return;
        }

        Debug.Log($"Task data loaded. Document count: {loadedWrapper.items.Count}");
        foreach (TaskDocument doc in loadedWrapper.items)
        {
            Debug.Log($"taskLevel = {doc.taskLevel}");
            foreach (string taskCode in doc.tasks)
            {
                Debug.Log($" - Task Code: {taskCode}");
            }
        }
    }

    // Firestore’dan gelen veriyi "loadedWrapper"a aktar
    public void SetDataFromFirestore(FirestoreDataCache.FirestoreTaskDocumentListWrapper wrapper)
    {
        if (wrapper == null || wrapper.items == null)
        {
            Debug.LogWarning("Firestore wrapper is null or has no items.");
            return;
        }

        List<TaskDocument> convertedList = new List<TaskDocument>();
        foreach (var fsDoc in wrapper.items)
        {
            TaskDocument localDoc = new TaskDocument
            {
                taskLevel = fsDoc.taskLevel,
                tasks     = fsDoc.tasks
            };
            convertedList.Add(localDoc);
        }

        loadedWrapper = new TaskDocumentListWrapper { items = convertedList };

        Debug.Log($"TasksManager: Firestore verileri alındı. Toplam doküman: {convertedList.Count}");
    }

    // Level'a ait görev kodlarını döndür
    public List<string> GetTasksForLevel(int level)
    {
        if (loadedWrapper?.items == null)
        {
            Debug.LogWarning("Data not loaded or invalid!");
            return new List<string>();
        }

        TaskDocument matchingDoc = loadedWrapper.items.Find(doc => doc.taskLevel == level);
        return matchingDoc?.tasks ?? new List<string>();
    }

    // "GxxTyyyAzzMwww" -> parse edip tabloya yaz
    public void ParseString(string input)
    {
        var match = parseRegex.Match(input);
        if (!match.Success)
        {
            throw new ArgumentException("Input string format is invalid: " + input);
        }

        int actualG = int.Parse(match.Groups["g"].Value); // ex: "03" -> 3
        int gIndex  = actualG - 1;

        if (gIndex < 0 || gIndex >= taskControllers.Count)
        {
            Debug.LogError($"ParseString: gIndex {gIndex} out of range!");
            return;
        }

        // Görev aktif
        taskControllers[gIndex][(int)TaskField.IsActive] = 1;

        // Target, Award, Amount
        taskControllers[gIndex][(int)TaskField.Target] = int.Parse(match.Groups["t"].Value);
        taskControllers[gIndex][(int)TaskField.Award]  = int.Parse(match.Groups["a"].Value);
        taskControllers[gIndex][(int)TaskField.Amount] = int.Parse(match.Groups["m"].Value);
    }

    // Tüm tabloyu sıfırla
    public void ResetTaskControllers()
    {
        foreach (var controller in taskControllers)
        {
            for (int i = 0; i < controller.Count; i++)
            {
                controller[i] = 0;
            }
        }
    }

    public int GetField(int gIndex, TaskField field)
    {
        return taskControllers[gIndex][(int)field];
    }

    public void MarkCompleted(int gIndex)
    {
        taskControllers[gIndex][(int)TaskField.IsCompleted] = 1;
        Debug.Log($"Task G{(gIndex+1):D2} is completed but reward not claimed yet.");
    }

    public void SetTaskFinish(int gIndex)
    {
        taskControllers[gIndex][(int)TaskField.IsActive]    = 0; 
        taskControllers[gIndex][(int)TaskField.IsCompleted] = 1;
        Debug.Log($"Task G{(gIndex+1):D2} reward claimed -> isActive=0, isCompleted=1.");
    }

    public bool IsActive(int gIndex)
    {
        return taskControllers[gIndex][(int)TaskField.IsActive] == 1;
    }

    public bool IsCompleted(int gIndex)
    {
        return taskControllers[gIndex][(int)TaskField.IsCompleted] == 1;
    }

    public void ResetAllTaskData()
    {
        ResetTaskControllers();
        
        for (int gIndex = 0; gIndex < taskControllers.Count; gIndex++)
        {
            string key = "rewardClaimed_G" + gIndex;
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
        
        // Diğer metrikleri sıfırla
        PlayerPrefs.SetInt("collectedLetter",   0);
        PlayerPrefs.SetInt("noJokerGames",      0);
        PlayerPrefs.SetInt("perfectGames",      0);
        PlayerPrefs.SetInt("useDiceJoker",      0);
        PlayerPrefs.SetInt("usePencilJoker",    0);
        PlayerPrefs.SetInt("useDictionaryJoker",0);
        PlayerPrefs.SetInt("useBrushJoker",     0);
        PlayerPrefs.Save();
        
        Debug.Log("All task data has been reset.");
    }
}

[Serializable]
public class TaskDocument
{
    public int taskLevel;
    public List<string> tasks;
}

[Serializable]
public class TaskDocumentListWrapper
{
    public List<TaskDocument> items;
}
