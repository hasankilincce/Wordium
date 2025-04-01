using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TasksUI : MonoBehaviour
{
    [Header("Managers & UI")]
    public TasksManager tasksManager;
    public ParticleSystemManager particleSystemManager;
    public ProgressBarUI progressBarUI;  // Opsiyonel

    [Header("Task Bars (G01 - G09 sıralamasıyla)")]
    public GameObject[] TaskBars;

    [Header("Scroll Content")]
    public GameObject content;

    public GameObject tasksComplatedPopup;
    public GameObject tasksComplatedText;

    /// <summary>
    /// Oyuncunun mevcut seviye görevlerini ekrana yükler.
    /// </summary>
    public void TaskLevelLoader()
    {
        // 1) tabloyu sıfırla
        tasksManager.ResetTaskControllers();

        // Mevcut level'ı al
        int currentLevel = PlayerPrefs.GetInt("taskLevel", 1);

        // O level'a ait görev kodlarını çek
        List<string> levelTasks = tasksManager.GetTasksForLevel(currentLevel);

        // Tüm task bar'ları gizle
        HideAllTaskBars();

        // Eğer o level'da hiç görev tanımlı değilse, 
        // direkt "tamamlandı" gibi görünüp bir sonraki seviye atlamasını engellemek için 
        // "otomatik atlama" yapmayacağız. Sadece uyarı basıyoruz.
        if (levelTasks == null || levelTasks.Count == 0)
        {
            Debug.LogWarning($"Level {currentLevel} has NO tasks. Not skipping automatically!");
            // İsterseniz “Pop-up” falan gösterebilirsiniz.
            return;
        }

        Debug.Log($"Level {currentLevel} has {levelTasks.Count} task(s).");

        // 2) Görevleri parse et & ödül durumlarını kontrol et
        foreach (string taskCode in levelTasks)
        {
            int gNumber = int.Parse(taskCode.Substring(1, 2));
            int gIndex  = gNumber - 1;

            // Ödül alındıysa kapat
            if (PlayerPrefs.GetInt($"rewardClaimed_G{gIndex}", 0) == 1)
            {
                tasksManager.SetTaskFinish(gIndex);
            }
            else
            {
                // Aksi halde parse edip aktif yap
                tasksManager.ParseString(taskCode);
            }
        }

        // 3) UI çizimi
        int completedCount = 0;
        for (int gIndex = 0; gIndex < tasksManager.taskControllers.Count; gIndex++)
        {
            bool isActive    = tasksManager.IsActive(gIndex);
            bool isCompleted = tasksManager.IsCompleted(gIndex);

            // Tamamen bitmiş (ödül alınmış) görev
            if (isCompleted && !isActive)
            {
                completedCount++;
            }

            // Görev aktif veya tamamlanmışsa ekranda göster
            if (isActive || isCompleted)
            {
                int levelType = gIndex + 1;
                int target    = tasksManager.GetField(gIndex, TasksManager.TaskField.Target);
                int current   = GetTaskProgress(levelType);
                bool meetsReq = (current >= target);

                PrepareTaskBar(levelType, target, current, meetsReq);
            }
        }

        // 4) Progress Bar (varsa)
        if (progressBarUI != null)
        {
            progressBarUI.SetProgress(completedCount, levelTasks.Count);
        }

        // 5) Eğer bu level'daki tüm görevler bittiyse => bir üst level
        if (completedCount == levelTasks.Count)
        {
            Debug.Log($"All tasks at level {currentLevel} are completed!");

            // ANİMASYONU BAŞLAT, BİTİNCE “onComplete” içinde ilerle
            TasksComplatedRoutine(() =>
            {
                // Animasyon bittiğinde bu kısım çalışır

                int nextLevel = currentLevel + 1;
                
                List<string> nextLevelTasks = tasksManager.GetTasksForLevel(nextLevel);
                if (nextLevelTasks == null || nextLevelTasks.Count == 0)
                {
                    Debug.LogWarning($"No tasks found for NEXT level {nextLevel}, not skipping automatically!");
                    return;
                }

                PlayerPrefs.SetInt("taskLevel", nextLevel);
                PlayerPrefs.Save();

                // Tabloları sıfırla
                tasksManager.ResetAllTaskData();

                // Yeni levelin görevlerini yükle
                TaskLevelLoader();
            });

            // return diyerek bu fonksiyonu bitirelim
            return;
        }


        // 6) Eğer hala görevler bitmediyse, Content boyutunu ayarla
        ResizeContent(levelTasks.Count);
    }

    /// <summary>
    /// Her bir görev bar’ını düzenleyen fonksiyon
    /// </summary>
    public void PrepareTaskBar(int levelType, int target, int current, bool meetsRequirement)
    {
        int gIndex = levelType - 1;
        if (gIndex < 0 || gIndex >= TaskBars.Length)
        {
            Debug.LogWarning($"PrepareTaskBar: levelType {levelType} out of range.");
            return;
        }

        // Parent objeyi mutlaka aç
        GameObject barObject = TaskBars[gIndex];
        if (barObject == null) return;
        barObject.SetActive(true);

        // Çocuk objeleri bul
        Transform mainTransform             = barObject.transform.Find("Main");
        Transform taskProgressTextTransform = mainTransform?.Find("TaskProgressText");
        Transform rewardButtonTransform     = barObject.transform.Find("TakeReward");
        Transform doneTransform             = barObject.transform.Find("Completed");

        // Hepsini kapatalım, sonra lazım olanı açacağız
        if (mainTransform != null)              mainTransform.gameObject.SetActive(false);
        if (taskProgressTextTransform != null)  taskProgressTextTransform.gameObject.SetActive(false);
        if (rewardButtonTransform != null)      rewardButtonTransform.gameObject.SetActive(false);
        if (doneTransform != null)              doneTransform.gameObject.SetActive(false);

        bool isActive    = tasksManager.IsActive(gIndex);
        bool isCompleted = tasksManager.IsCompleted(gIndex);

        // Ödül tamamen alınmış görev (isCompleted && !isActive)
        if (isCompleted && !isActive)
        {
            // "Completed" yazısını aç
            if (doneTransform != null)
                doneTransform.gameObject.SetActive(true);

            // Bu bar’ı en alt sıraya al
            barObject.transform.SetAsLastSibling();
            return;
        }

        // Görev devam ediyorsa "Main" kısmını görünür yap
        if (mainTransform != null)
            mainTransform.gameObject.SetActive(true);

        // Eğer hedefe ulaşıldıysa ve hala aktifse => “Ödül Al” butonu
        if (meetsRequirement && isActive)
        {
            if (rewardButtonTransform != null)
            {
                rewardButtonTransform.gameObject.SetActive(true);
                Button button = rewardButtonTransform.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnCompletedClicked(levelType));
                }
            }

            // İlerleme metnini isterseniz gizleyebilirsiniz
            if (taskProgressTextTransform != null)
                taskProgressTextTransform.gameObject.SetActive(false);
        }
        else
        {
            // Henüz hedefe ulaşılmadıysa => sayacı göster
            if (taskProgressTextTransform != null)
            {
                taskProgressTextTransform.gameObject.SetActive(true);
                TMP_Text txt = taskProgressTextTransform.GetComponent<TMP_Text>();
                if (txt != null)
                    txt.text = $"{current}/{target}";
            }
            // Ödül Al butonunu kapalı tut
            if (rewardButtonTransform != null)
                rewardButtonTransform.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// "Ödül Al" butonuna basıldığında
    /// </summary>
    private void OnCompletedClicked(int levelType)
    {
        int gIndex = levelType - 1;
        Debug.Log($"Clicked Completed for G{(gIndex+1):D2} -> Claiming reward...");

        // Görevi bitir
        tasksManager.SetTaskFinish(gIndex);

        // PlayerPrefs’e ödül alındı işareti
        PlayerPrefs.SetInt($"rewardClaimed_G{gIndex}", 1);
        PlayerPrefs.Save();

        // UI tekrar yükle
        TaskLevelLoader();
    }

    /// <summary>
    /// O görevin PlayerPrefs’teki ilerleme sayacını getirir.
    /// </summary>
    public int GetTaskProgress(int levelType)
    {
        switch (levelType)
        {
            case 1: return PlayerPrefs.GetInt("level", 0);
            case 2: return PlayerPrefs.GetInt("collectedLetter", 0);
            case 3: return PlayerPrefs.GetInt("strikeCount", 0);
            case 4: return PlayerPrefs.GetInt("noJokerGames", 0);
            case 5: return PlayerPrefs.GetInt("perfectGames", 0);
            case 6: return PlayerPrefs.GetInt("useDiceJoker", 0);
            case 7: return PlayerPrefs.GetInt("usePencilJoker", 0);
            case 8: return PlayerPrefs.GetInt("useDictionaryJoker", 0);
            case 9: return PlayerPrefs.GetInt("useBrushJoker", 0);
            default: return 0;
        }
    }

    /// <summary>
    /// Tüm TaskBars’ı kapatır.
    /// </summary>
    private void HideAllTaskBars()
    {
        foreach (GameObject bar in TaskBars)
        {
            if (bar != null) 
                bar.SetActive(false);
        }
    }

    /// <summary>
    /// Scroll Content boyutunu görev sayısına göre ayarlar
    /// </summary>
    private void ResizeContent(int taskCount)
    {
        if (content == null) 
            return;

        RectTransform contentRect = content.GetComponent<RectTransform>();
        if (contentRect == null) 
            return;

        int newHeight = 360 * taskCount + 360;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, newHeight);

        Vector2 anchoredPos = contentRect.anchoredPosition;
        anchoredPos.y = 0;
        contentRect.anchoredPosition = anchoredPos;
    }


    private void TasksComplatedRoutine(System.Action onComplete)
    {
        tasksComplatedPopup.SetActive(true);
        tasksComplatedText.transform.localScale = Vector3.zero;

        tasksComplatedText.transform
            .DOScale(Vector3.one, 1.8f)
            .SetEase(Ease.OutBack)
            // Animasyon bitince şunu yap:
            .OnComplete(() => 
            {
                tasksComplatedPopup.SetActive(false);
                tasksComplatedText.transform.localScale = Vector3.one;
                // Dışarıdan gelen "onComplete" eylemini çağır:
                onComplete?.Invoke();
            });

        // Partikül efekti
        particleSystemManager.FireWorkCaller();
    }

}
