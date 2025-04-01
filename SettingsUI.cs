using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SettingsUI : MonoBehaviour
{
    public Toggle soundToggle;
    public Toggle vibrationToggle;
    public Toggle notificationToggle;
    public GameObject DeleteDataPanel;

    public static event Action<bool> OnNotificationSettingsChanged;

    void Start()
    {
        int soundValue = PlayerPrefs.GetInt("sound", 1);
        soundToggle.isOn = soundValue == 1;
        soundToggle.UpdateVisualImmediately();

        int vibrationValue = PlayerPrefs.GetInt("vibration", 1);
        vibrationToggle.isOn = vibrationValue == 1;
        vibrationToggle.UpdateVisualImmediately();

        int notificationValue = PlayerPrefs.GetInt("notification", 1);
        notificationToggle.isOn = notificationValue == 1;
        notificationToggle.UpdateVisualImmediately();
    }

    public void ShowDeleteDataPanel()
    {
        DeleteDataPanel.SetActive(true);
    }

    public void HideDeleteDataPanel()
    {
        DeleteDataPanel.SetActive(false);
    }

    public void ToggleSound(bool isOn)
    {
        Debug.Log(isOn ? "Ses Açıldı" : "Ses Kapatıldı");
        PlayerPrefs.SetInt("sound", isOn ? 1 : 0);
    }

    public void ToggleVibration(bool isOn)
    {
        Debug.Log(isOn ? "Titreşim Açıldı" : "Titreşim Kapatıldı");
        PlayerPrefs.SetInt("vibration", isOn ? 1 : 0);
    }

    public void ToggleNotification(bool isOn)
    {
        Debug.Log(isOn ? "Bildirimler Açıldı" : "Bildirimler Kapatıldı");
        PlayerPrefs.SetInt("notification", isOn ? 1 : 0);
        OnNotificationSettingsChanged?.Invoke(isOn);
    }
}
