using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public GameManager gameManager;
    public UserDataManager userDataManager;
    public HealthManager healthManager;
    public NotificationManager notificationManager;

    public GameObject[] cardCells;
    public GameEndUI gameEndUI;

    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject navigationBar;

    public TMP_Text coinText;
    public TMP_Text healthText;
    public TMP_Text levelText;




    public void MainMenuStart(){
        gameEndUI.CloseGameEndPanel();
        
        NormalizeCoinCountText(coinText);
        healthText.text = PlayerPrefs.GetInt("health").ToString();

        levelText.text = "SEVİYE " + PlayerPrefs.GetInt("level").ToString();

        gameManager.ConfigureCardCells(cardCells, PlayerPrefs.GetInt("level"));

        notificationManager.ShowSeriesBar();
    }

    public void GameRestart(){
        if (healthManager.isHealthCountEnough())
        {
            mainMenuPanel.SetActive(false);
            navigationBar.SetActive(false);
            gameManager.GameRestart(PlayerPrefs.GetInt("level"));
        }
        else{
            Debug.Log("Yetersiz Can!");
        }

    }

    public void ReturnMainMenu(){
        mainMenuPanel.SetActive(true);
        navigationBar.SetActive(true);
        settingsPanel.SetActive(false);
        gameManager.gameAreaPanel.SetActive(false);
        MainMenuStart();
    }

    public void OpenSettingsPanel(){
        settingsPanel.SetActive(true);
    }


    public void NormalizeCoinCountText(TMP_Text coinText)
    {
        int coinCount = PlayerPrefs.GetInt("coins");

        if (coinCount < 1000)
        {
            coinText.text = coinCount.ToString();
        }
        else if (coinCount < 10000)
        {
            // 1.000 ile 9.999 arası: Değeri 1000'e böl ve floor ile yuvarla
            float value = coinCount / 1000f;
            float truncated = Mathf.Floor(value * 10f) / 10f;
            coinText.text = truncated.ToString("0.#") + "K";
        }
        else if (coinCount < 1000000)
        {
            // 10.000 ile 999.999 arası: Tam sayı olarak göster
            coinText.text = (coinCount / 1000).ToString() + "K";
        }
        else if (coinCount < 10000000)
        {
            // 1M ile 9.999.999 arası: Değeri 1M'e böl ve floor ile yuvarla
            float value = coinCount / 1000000f;
            float truncated = Mathf.Floor(value * 10f) / 10f;
            coinText.text = truncated.ToString("0.#") + "M";
        }
        else
        {
            // 10M ve üzeri: Tam sayı olarak göster
            coinText.text = (coinCount / 1000000).ToString() + "M";
        }
    }


}
