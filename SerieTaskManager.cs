using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SerieTaskManager : MonoBehaviour
{
    [Header("References")]
    public TasksManager tasksManager;
    public TasksUI tasksUI;

    [Header("Göreve Karşılık Gelen Index (Ör. G01 => 0, G02 => 1 vb.)")]
    public int serieTaskIndex;  

    // Sayfa referansları
    private GameObject mainPage;
    private GameObject rewardsPage;
    private GameObject takeRewardPage;
    private GameObject infoPage;

    // Butonlar
    private Button infoButton;
    private Button showRewardButton;
    private Button rewardsPageButton;
    private Button takeRewardPageButton;
    private Button infoPageButton;

    [Header("Ödül Prefab Objeleri (Inspector’dan atayın)")]
    public GameObject coinReward;
    public GameObject diceReward;
    public GameObject pencilReward;
    public GameObject dictionaryReward;
    public GameObject brushReward;

    private void Awake()
    {
        // Alt obje referanslarını bulalım
        mainPage       = transform.Find("Main")?.gameObject;
        rewardsPage    = transform.Find("Rewards")?.gameObject;
        takeRewardPage = transform.Find("TakeReward")?.gameObject;
        infoPage       = transform.Find("Info")?.gameObject;

        // Ana sayfadaki butonlar
        infoButton       = transform.Find("Main/InfoButton")?.GetComponent<Button>();
        showRewardButton = transform.Find("Main/ShowRewardButton")?.GetComponent<Button>();

        // Sayfaların kendi butonları
        if (rewardsPage != null)
            rewardsPageButton = rewardsPage.GetComponent<Button>();
        if (takeRewardPage != null)
            takeRewardPageButton = takeRewardPage.GetComponent<Button>();
        if (infoPage != null)
            infoPageButton = infoPage.GetComponent<Button>();
    }

    private void Start()
    {
        StartRoutine();
    }

    public void StartRoutine()
    {
        // 1) Bu gIndex görevi aktif mi?
        bool isActive = tasksManager.IsActive(serieTaskIndex);
        if (!isActive)
        {
            // Eğer bu level'da Gxx yoksa tasksManager parse etmemiştir ve tablo 0. 
            // Hiç gösterme:
            Debug.Log($"[SerieTaskManager] G{(serieTaskIndex+1):D2} is not active this level => disabling object.");
            gameObject.SetActive(false);
            return;
        }

        HideAllPages();

        if (mainPage != null) 
            mainPage.SetActive(true);

        if (infoButton != null)
            infoButton.onClick.AddListener(OnInfoButtonClicked);

        if (showRewardButton != null)
            showRewardButton.onClick.AddListener(OnShowRewardButtonClicked);

        if (rewardsPageButton != null)
            rewardsPageButton.onClick.AddListener(ShowMainPage);
        if (takeRewardPageButton != null)
            takeRewardPageButton.onClick.AddListener(ShowMainPage);
        if (infoPageButton != null)
            infoPageButton.onClick.AddListener(ShowMainPage);

        // Debug amaçlı son durum
        int awardType   = tasksManager.GetField(serieTaskIndex, TasksManager.TaskField.Award);
        int awardAmount = tasksManager.GetField(serieTaskIndex, TasksManager.TaskField.Amount);
        Debug.Log($"SerieTaskManager (G{(serieTaskIndex+1):D2}) başlatıldı. " +
                  $"Award={awardType}, Amount={awardAmount}");
    }

    private void HideAllPages()
    {
        if (mainPage)       mainPage.SetActive(false);
        if (rewardsPage)    rewardsPage.SetActive(false);
        if (takeRewardPage) takeRewardPage.SetActive(false);
        if (infoPage)       infoPage.SetActive(false);
    }

    private void OnInfoButtonClicked()
    {
        HideAllPages();
        if (infoPage != null)
            infoPage.SetActive(true);
    }

    /// <summary>
    /// "Ödüller" butonuna tıklandığında
    /// </summary>
    private void OnShowRewardButtonClicked()
    {
        Debug.Log($"OnShowRewardButtonClicked => G{serieTaskIndex+1:D2}");
        
        // Normalde bu noktaya sadece "isActive" = true durumunda gelebiliriz.
        HideAllPages();

        if (rewardsPage != null) 
            rewardsPage.SetActive(true);

        // Tüm ödülleri kapat
        if (coinReward)       coinReward.SetActive(false);
        if (diceReward)       diceReward.SetActive(false);
        if (pencilReward)     pencilReward.SetActive(false);
        if (dictionaryReward) dictionaryReward.SetActive(false);
        if (brushReward)      brushReward.SetActive(false);

        // Ödül tipini & miktarını çek
        int awardType   = tasksManager.GetField(serieTaskIndex, TasksManager.TaskField.Award);
        int awardAmount = tasksManager.GetField(serieTaskIndex, TasksManager.TaskField.Amount);
        Debug.Log($"[SerieTaskManager] G{serieTaskIndex+1:D2} -> awardType={awardType}, amount={awardAmount}");

        string amountText = awardAmount.ToString();

        switch (awardType)
        {
            case 1: // Coin
                if (coinReward)
                {
                    coinReward.SetActive(true);
                    var t = coinReward.transform.Find("RewardAmount");
                    if (t != null)
                    {
                        var tmpText = t.GetComponent<TMP_Text>();
                        if (tmpText) tmpText.SetText(amountText);
                    }
                }
                break;

            case 2: // Dice
                if (diceReward)
                {
                    diceReward.SetActive(true);
                    var t = diceReward.transform.Find("RewardAmount");
                    if (t != null)
                    {
                        var tmpText = t.GetComponent<TMP_Text>();
                        if (tmpText) tmpText.SetText(amountText);
                    }
                }
                break;

            case 3: // Pencil
                if (pencilReward)
                {
                    pencilReward.SetActive(true);
                    var t = pencilReward.transform.Find("RewardAmount");
                    if (t != null)
                    {
                        var tmpText = t.GetComponent<TMP_Text>();
                        if (tmpText) tmpText.SetText(amountText);
                    }
                }
                break;

            case 4: // Dictionary
                if (dictionaryReward)
                {
                    dictionaryReward.SetActive(true);
                    var t = dictionaryReward.transform.Find("RewardAmount");
                    if (t != null)
                    {
                        var tmpText = t.GetComponent<TMP_Text>();
                        if (tmpText) tmpText.SetText(amountText);
                    }
                }
                break;

            case 5: // Brush
                if (brushReward)
                {
                    brushReward.SetActive(true);
                    var t = brushReward.transform.Find("RewardAmount");
                    if (t != null)
                    {
                        var tmpText = t.GetComponent<TMP_Text>();
                        if (tmpText) tmpText.SetText(amountText);
                    }
                }
                break;

            default:
                Debug.LogWarning($"[SerieTaskManager] Bilinmeyen ödül tipi: {awardType}");
                break;
        }
    }

    public void ShowMainPage()
    {
        HideAllPages();
        if (mainPage != null) 
            mainPage.SetActive(true);
    }

    /// <summary>
    /// "Ödül Al" butonu tıklanınca -> Görevi finalize
    /// </summary>
    public void TakeRewardButtonClicked()
    {
        // Görevi bitmiş (ödül alınmış) olarak işaretle
        tasksManager.SetTaskFinish(serieTaskIndex);
        PlayerPrefs.SetInt("rewardClaimed_G" + serieTaskIndex, 1);
        PlayerPrefs.Save();

        // Burada, gerçekten "ödülü karaktere ekleme" vb. işlemleri yapacağınız fonksiyon:
        OnRewardCollected();

        // Görevler sayfasını (TasksUI) yenile
        if (tasksUI != null)
            tasksUI.TaskLevelLoader();

        // Kendi sayfamızda "TakeReward" sayfasını açalım
        HideAllPages();
        if (takeRewardPage != null)
            takeRewardPage.SetActive(true);
    }

    /// <summary>
    /// Burayı siz doldurabilirsiniz. Örneğin coin ekleme, item envantere ekleme vs.
    /// </summary>
    private void OnRewardCollected()
    {
        // Örnek:
        // PlayerCoinManager.instance.AddCoins( X );
        // InventoryManager.instance.AddItem("Dice", X);
        // vs.
        Debug.Log("OnRewardCollected() -> Ödül ekleme mantığını burada uygulayabilirsiniz.");
    }
}
