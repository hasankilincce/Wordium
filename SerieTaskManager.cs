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

    [Header("Göreve Karşılık Gelen Index (Ör. G02 => 1, G03 => 2 vb.)")]
    public int serieTaskIndex;  
    // Not: G01 => index=0, G02 => index=1, G03 => index=2, vb.

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
        // Alt obje referanslarını bulalım (transform.Find yolunu kendinize göre düzenleyin)
        mainPage       = transform.Find("Main")?.gameObject;
        rewardsPage    = transform.Find("Rewards")?.gameObject;
        takeRewardPage = transform.Find("TakeReward")?.gameObject;
        infoPage       = transform.Find("Info")?.gameObject;

        // Ana sayfadaki butonlar
        infoButton       = transform.Find("Main/InfoButton")?.GetComponent<Button>();
        showRewardButton = transform.Find("Main/ShowRewardButton")?.GetComponent<Button>();

        // Sayfaların kendi butonları (eğer tek bir Button component’ı varsa)
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
        Debug.Log("ShowRewardButton clicked");
        HideAllPages();

        // "Rewards" sayfasını göster
        if (rewardsPage != null) 
            rewardsPage.SetActive(true);

        // Tüm ödülleri kapat
        if (coinReward)       coinReward.SetActive(false);
        if (diceReward)       diceReward.SetActive(false);
        if (pencilReward)     pencilReward.SetActive(false);
        if (dictionaryReward) dictionaryReward.SetActive(false);
        if (brushReward)      brushReward.SetActive(false);

        // Görevin ödül tipini ve miktarını al
        int awardType   = tasksManager.GetField(serieTaskIndex, TasksManager.TaskField.Award);
        int awardAmount = tasksManager.GetField(serieTaskIndex, TasksManager.TaskField.Amount);
        string amountText = awardAmount.ToString();

        // İlgili ödülü aç ve miktar metnini güncelle
        switch (awardType)
        {
            case 1: // Coin
                if (coinReward)
                {
                    coinReward.SetActive(true);
                    coinReward.transform.Find("RewardAmount")?.GetComponent<TMP_Text>()?.SetText(amountText);
                }
                break;
            case 2: // Dice
                if (diceReward)
                {
                    diceReward.SetActive(true);
                    diceReward.transform.Find("RewardAmount")?.GetComponent<TMP_Text>()?.SetText(amountText);
                }
                break;
            case 3: // Pencil
                if (pencilReward)
                {
                    pencilReward.SetActive(true);
                    pencilReward.transform.Find("RewardAmount")?.GetComponent<TMP_Text>()?.SetText(amountText);
                }
                break;
            case 4: // Dictionary
                if (dictionaryReward)
                {
                    dictionaryReward.SetActive(true);
                    dictionaryReward.transform.Find("RewardAmount")?.GetComponent<TMP_Text>()?.SetText(amountText);
                }
                break;
            case 5: // Brush
                if (brushReward)
                {
                    brushReward.SetActive(true);
                    brushReward.transform.Find("RewardAmount")?.GetComponent<TMP_Text>()?.SetText(amountText);
                }
                break;
            default:
                Debug.LogWarning($"Bilinmeyen ödül tipi: {awardType}");
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
        Debug.Log("OnRewardCollected() fonksiyonu tetiklendi. Buraya ödül ekleme mantığını yazabilirsiniz.");
    }
}
