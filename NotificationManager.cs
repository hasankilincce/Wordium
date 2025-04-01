using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public GameObject NotificationPanel;

    public GameObject SeriesBar;
    public GameObject NoAdsBar;
    public GameObject DailyTaskBar;



    public TMP_Text SeriesCountText;


    public void ShowSeriesBar(){
        if (PlayerPrefs.GetInt("strikeCount") > 0)
        {
            NotificationPanel.SetActive(true);
            SeriesBar.SetActive(true);
            SeriesCountText.text = PlayerPrefs.GetInt("strikeCount").ToString();
        }
        else
        {
            NotificationPanel.SetActive(false);
            SeriesBar.SetActive(false);
        } 
    }

    public void ShowNoAdsBar(){
        NoAdsBar.SetActive(true);
    }

    public void ShowDailyTaskBar(){
        DailyTaskBar.SetActive(true);
    }
}
