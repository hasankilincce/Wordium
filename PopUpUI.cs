using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;

public class PopUpUI : MonoBehaviour
{
    public AdManager adManager;
    public MainMenuUI mainMenuUI;
    public GameManager gameManager;
    public IncrementTimer incrementTimer;

    public GameObject popUpPanel;
    public GameObject topPanel;
    public GameObject extraMovePanel;
    public GameObject fullHealthPanel;
    public GameObject waitHealthPanel;
    public GameObject upgradeAppPanel;

    public GameObject healthBar;
    public GameObject coinBar;
    public GameObject strikeBar;

    public TMP_Text timeText;
    public TMP_Text coinText;
    public TMP_Text strikeCountText;
    public TMP_Text healthText;

    private Coroutine timerCoroutine;


    public void OpenPopUpPanel(){
        ActivateWithFade(popUpPanel);
        mainMenuUI.NormalizeCoinCountText(coinText);
        strikeCountText.text = PlayerPrefs.GetInt("strikeCount").ToString();
    }

    public void ClosePopUpPanel(){
        popUpPanel.SetActive(false);
        CloseTopPanel();
    }

    public void ShowExtraMovePanel(){
        OpenPopUpPanel();
        ActivateWithBoingEffect(extraMovePanel);

        ShowTopPanel(true, false, true);
    }

    public void CloseExtraMovePanel(){
        ClosePopUpPanel();
        extraMovePanel.SetActive(false);

        gameManager.isGameFinished();
    }

    public void ShowUpgradeAppPanel(){
        OpenPopUpPanel();
        ActivateWithBoingEffect(upgradeAppPanel);
    }


    // EXTRA MOVE PANEL ANIMATIONS
    public void WatchAdForExtraMove(){
        ClosePopUpPanel();
        adManager.ShowRewardedAd(5, true);
    }

    public void BuyExtraMove(){
        int coin = PlayerPrefs.GetInt("coins");

        if (coin >= 2000)
        {
            gameManager.moveCountUpdate(5);
            coin -= 2000;
            PlayerPrefs.SetInt("coins", coin);
            mainMenuUI.NormalizeCoinCountText(coinText);

            ClosePopUpPanel();
        }

    }

    // HEALTH PANEL ANIMATIONS
    public void ShowHealthPanel(){
        OpenPopUpPanel();
        if (PlayerPrefs.GetInt("health") < 5)
        {
            ActivateWithBoingEffect(waitHealthPanel);
            StartTimerAnimation();

            healthText.text = PlayerPrefs.GetInt("health").ToString();

            ShowTopPanel(false, true, false);
        }
        else
        {
            ActivateWithBoingEffect(fullHealthPanel);
        }
    }
    
    public void StartTimerAnimation()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(PlayTimerAnimation());
    }

    public void StopTimerAnimation()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
    }

    IEnumerator PlayTimerAnimation()
    {
        while (true)
        {
            float remainingSeconds = incrementTimer.GetSecondsToNextIncrement();
            
            int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60f);

            timeText.text = $"{minutes:D2}:{seconds:D2}";

            yield return new WaitForSeconds(1.0f);
        }
    }

    public void WatchAdForHealth(){
        adManager.ShowRewardedAd(1, false);

        healthText.text = PlayerPrefs.GetInt("health").ToString();
    }

    public void CloseFullHealthPanel(){
        ClosePopUpPanel();
        fullHealthPanel.SetActive(false);
        waitHealthPanel.SetActive(false);

        mainMenuUI.MainMenuStart();
    }

    public void CloseWaitHealthPanel(){
        ClosePopUpPanel();
        fullHealthPanel.SetActive(false);
        waitHealthPanel.SetActive(false);

        mainMenuUI.MainMenuStart();

        StopCoroutine(timerCoroutine);
    }


    // TOP PANEL
    public void ShowTopPanel(bool coin, bool health, bool strike){
        ActivateWithFade(topPanel);
        if (coin)
        {
            coinBar.SetActive(true);
        }
        if (health)
        {
            healthBar.SetActive(true);
        }
        if (strike)
        {
            strikeBar.SetActive(true);
        }
    }

    public void CloseTopPanel(){
        topPanel.SetActive(false);
        coinBar.SetActive(false);
        healthBar.SetActive(false);
        strikeBar.SetActive(false);
    }

    public void ActivateWithBoingEffect(GameObject target, float duration = 0.5f)
    {
        if (target == null) return;
        
        // Nesneyi aktif hale getir
        target.SetActive(true);

        // Başlangıçta ölçeği sıfır yap
        target.transform.localScale = Vector3.zero;

        // Ölçeği büyütüp küçülterek boing efekti verme
        target.transform.DOScale(1.1f, duration * 0.4f)  // Önce %110 büyüt
            .SetEase(Ease.OutBack)                        // Yumuşak sıçrama efekti
            .OnComplete(() => 
                target.transform.DOScale(1f, duration * 0.3f) // Sonra %100 boyuta getir
                .SetEase(Ease.InOutBounce)
            );
    }


    public void ActivateWithFade(GameObject target, float duration = 0.5f)
    {
        if (target == null) return;

        // Nesneyi aktif hale getir
        target.SetActive(true);

        // Eğer GameObject'te bir SpriteRenderer varsa (2D oyunlar için)
        SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            startColor.a = 0; // Saydam başlat
            spriteRenderer.color = startColor;

            // Saydamdan opak hale getir
            spriteRenderer.DOFade(1f, duration);
            return;
        }

        // Eğer GameObject'te CanvasGroup varsa (UI nesneleri için)
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0; // Saydam başlat
            canvasGroup.DOFade(1f, duration);
            return;
        }

        Debug.LogWarning("GameObject'te SpriteRenderer veya CanvasGroup bulunamadı!");
    }

}
