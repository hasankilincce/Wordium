using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellAnimations : MonoBehaviour
{
    public SoundManager soundManager;
    public VibrationManager vibrationManager;

    
    // Hücrelerin üzerine gelindiğinde çalışacak fonksiyon
    public void OnCellHovered(GameObject cell, Color BGColor, Color TextColor)
    {
        if (PlayerPrefs.GetInt("vibration") == 1)
        {
            vibrationManager.SoftVibrate();
        }
        if (BGColor != new Color(0.1333333f, 0.1333333f, 0.1333333f))
        {
            cell.GetComponent<Image>().color = LightenColor(BGColor);
        }
        else
        {
            cell.GetComponent<Image>().color = Color.white;
        }
        
        // "CellBG" nesnesini bul
        Transform cellBGTransform = cell.transform.Find("CellBG");
        if (cellBGTransform != null)
        {
            Image cellBGImage = cellBGTransform.GetComponent<Image>();
            if (cellBGImage != null)
            {
                cellBGImage.color = BGColor;
            }
            else
            {
                Debug.LogWarning("CellBG nesnesinde Image bileşeni bulunamadı!");
            }
        }
        else
        {
            Debug.LogWarning("CellBG nesnesi bulunamadı!");
        }

        // "CellText" nesnesini bul
        Transform cellTextTransform = cell.transform.Find("CellText");
        if (cellTextTransform != null)
        {
            TextMeshProUGUI cellText = cellTextTransform.GetComponent<TextMeshProUGUI>();
            if (cellText != null)
            {
                cellText.color = TextColor;
            }
            else
            {
                Debug.LogWarning("CellText nesnesinde TextMeshProUGUI bileşeni bulunamadı!");
            }
        }
        else
        {
            Debug.LogWarning("CellText nesnesi bulunamadı!");
        }
    }


    public void CellAnimation(GameObject cell, Color BGColor, Color TextColor)
    {
        soundManager.PlayPopSound();

        // Önce hücrede var mı diye bakıyoruz
        Canvas canvas = cell.GetComponent<Canvas>();
        bool newlyAdded = false;

        // Eğer Canvas yoksa biz ekleyelim
        if (canvas == null)
        {
            canvas = cell.AddComponent<Canvas>();
            newlyAdded = true;
        }

        // Canvas sıralama ayarları
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10;

        // Renkleri ayarla
        OnCellHovered(cell, BGColor, TextColor);

        // DOTween animasyonu
        Sequence sequence = DOTween.Sequence();
        sequence.Append(
            cell.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f)
                .SetEase(Ease.OutBounce))
        .Append(
            cell.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.InOutQuad))
        .OnComplete(() =>
        {
            // Animasyon bitince sıralamayı sıfırla
            canvas.sortingOrder = 0;
            canvas.overrideSorting = false;

            // Eğer bu Canvas'ı yeni eklemişsek sil
            if (newlyAdded && canvas != null)
            {
                Destroy(canvas);
            }
        });
    }


    public Color DarkenColor(Color color, float factor = 0.8f)
    {
        return new Color(
            Mathf.Clamp01(color.r * factor), 
            Mathf.Clamp01(color.g * factor), 
            Mathf.Clamp01(color.b * factor),
            color.a // Alfa değerini değiştirmiyoruz
        );
    }

    public Color LightenColor(Color color, float factor = 1.2f)
    {
        return new Color(
            Mathf.Clamp01(color.r * factor),
            Mathf.Clamp01(color.g * factor),
            Mathf.Clamp01(color.b * factor),
            color.a // Alfa değerini değiştirmiyoruz
        );
    }

}
