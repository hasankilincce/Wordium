using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public GameManager gameManager;
    public JokerAnimations jokerAnimations;
    public JokerManager jokerManager;
    public UserDataManager userDataManager;

    public Image JokerUsePanel;
    public Image PencilJokerImage;
    public Image DictionaryJokerImage;

    public TMP_Text diceJokerCountText;
    public TMP_Text pencilJokerCountText;
    public TMP_Text dictionaryJokerCountText;
    public TMP_Text brushJokerCountText;


    public void DiceJokerButton(){
        if(!gameManager.IsGameEnd()){
            if (jokerManager.IsUserJokerEnough(0))
            {
                jokerAnimations.DiceJokerClicked();
                jokerManager.ChangeJokerCount(0, -1);
                UpdateJokerCountTexts();

                gameManager.noJokerGames++; // Oyunun joker kullanılarak oynanma sayısını arttır
            }
        }
    }

    public void PencilJokerButton(){
        if(!gameManager.IsGameEnd()){
            if (jokerManager.IsUserJokerEnough(1))
            {
                jokerAnimations.PencilJokerClicked();

                gameManager.noJokerGames++; // Oyunun joker kullanılarak oynanma sayısını arttır
            }
        }
    }

    public void DictionaryJokerButton(){
        if(!gameManager.IsGameEnd()){
            if (jokerManager.IsUserJokerEnough(2))
            {
                jokerAnimations.DictionaryJokerClicked();

                gameManager.noJokerGames++; // Oyunun joker kullanılarak oynanma sayısını arttır
            }
        }   
    }

    public void BrushJokerButton(){
        if(!gameManager.IsGameEnd()){
            if (jokerManager.IsUserJokerEnough(3)){
                if(!gameManager.isBrushJokerUsed){
                    jokerAnimations.BrushJokerClicked();
                    jokerManager.ChangeJokerCount(3, -1);
                    UpdateJokerCountTexts();

                    gameManager.noJokerGames++; // Oyunun joker kullanılarak oynanma sayısını arttır
                }
            }
        }
    }


    public void ShowUseJokerPanel(){
        StartCoroutine(ShowUseJokerPanelRoutine()); 
    }

    public IEnumerator ShowUseJokerPanelRoutine(){
        yield return new WaitForSeconds(0.1f);
        ShowImage(JokerUsePanel);

        yield return new WaitForSeconds(0.1f);
        if (jokerManager.isPencilJoker)
        {
            ShowWithBoingEffect(PencilJokerImage);
        }
        else if(jokerManager.isDictionaryJoker){
            ShowWithBoingEffect(DictionaryJokerImage);
        }
    }



    public void HideUseJokerPanel(){
        StartCoroutine(HideUseJokerPanelRoutine());

        PencilJokerImage.gameObject.SetActive(false);
        DictionaryJokerImage.gameObject.SetActive(false);

    }

    public IEnumerator HideUseJokerPanelRoutine(){
        yield return new WaitForSeconds(0.1f);
        HideImage(JokerUsePanel);
    }


    public void CloseJoker(){
        if (jokerManager.isPencilJoker)
        {
            jokerAnimations.PencilJokerClicked();
            
        }
        else if(jokerManager.isDictionaryJoker){
            jokerAnimations.DictionaryJokerClicked();
            
        }
        jokerManager.isPencilJoker = false;
        jokerManager.isDictionaryJoker = false;
    }

    public void ShowImage(UnityEngine.UI.Image uiImage, float duration = 1f)
    {
        if (uiImage == null)
        {
            Debug.LogError("ShowImage() metodu: uiImage NULL!");
            return;
        }

        uiImage.gameObject.SetActive(true);

        // Başlangıç alfa değerini sıfıra çek (Eğer görünmez başlıyorsa)
        Color tempColor = uiImage.color;
        tempColor.a = 0;
        uiImage.color = tempColor;

        // Fade-in animasyonunu başlat
        uiImage.DOFade(0.77f, duration);
    }


    public void HideImage(UnityEngine.UI.Image uiImage, float duration = 0.5f)
    {
        if (uiImage == null)
        {
            Debug.LogError("ShowImage() metodu: uiImage NULL!");
            return;
        }

        uiImage.gameObject.SetActive(false);
        uiImage.DOFade(0, duration);
    }


    public void ShowWithBoingEffect(UnityEngine.UI.Image uiImage, float duration = 0.5f)
    {
        if (uiImage == null)
        {
            Debug.LogError("uiImage atanmadı!");
            return;
        }
        // Başlangıçta Image'i küçük yap (tamamen görünmez hale getirme)
        uiImage.transform.localScale = Vector3.zero;

        // Önce nesneyi aktif hale getir
        uiImage.gameObject.SetActive(true);

        // Boing efekti ile büyütme
        uiImage.transform.DOScale(1f, duration)
            .SetEase(Ease.OutBack); // "Boing" efekti için `OutBack` easing kullanıyoruz
    }


    public void UpdateJokerCountTexts(){
        diceJokerCountText.text = PlayerPrefs.GetInt("diceJoker").ToString();
        pencilJokerCountText.text = PlayerPrefs.GetInt("pencilJoker").ToString();
        dictionaryJokerCountText.text = PlayerPrefs.GetInt("dictionaryJoker").ToString();
        brushJokerCountText.text = PlayerPrefs.GetInt("brushJoker").ToString();
    }
}
