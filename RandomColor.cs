using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColor : MonoBehaviour
{
    private Color[] baseColors = new Color[]
{
    new Color(1.00f, 0.60f, 0.60f),   // CANLI PASTEL PEMBE
    new Color(0.50f, 0.75f, 1.00f),   // CANLI PASTEL MAVİ
    new Color(0.75f, 0.50f, 1.00f),   // CANLI LAVANTA MORU
    new Color(1.00f, 0.65f, 0.40f),   // CANLI ŞEFTALİ TURUNCU
    new Color(0.40f, 0.90f, 0.60f),   // CANLI NANE YEŞİLİ
    new Color(1.00f, 0.90f, 0.40f),   // CANLI LİMON SARISI
    new Color(0.30f, 0.70f, 1.00f),   // CANLI BEBEK MAVİSİ
    new Color(1.00f, 0.40f, 0.70f),   // CANLI PASTEL GÜL KURUSU
    new Color(0.30f, 1.00f, 0.60f),   // CANLI MİNT YEŞİLİ
    new Color(1.00f, 0.80f, 0.30f)    // CANLI AÇIK KREM
};


    private List<Color> availableColors;

    private void Awake()
    {
        ResetColorList();
    }

    public Color GetRandomColor()
    {
        if (availableColors.Count == 0)
        {
            //Debug.LogWarning("Tüm renkler kullanıldı! Liste sıfırlanıyor...");
            ResetColorList();
        }

        // Rastgele index seç
        int randomIndex = Random.Range(0, availableColors.Count);

        // Rengi al
        Color selectedColor = availableColors[randomIndex];

        // O rengi listeden çıkar
        availableColors.RemoveAt(randomIndex);

        // Seçilen rengi döndür
        return selectedColor;
    }

    private void ResetColorList()
    {
        // Yeni bir liste oluşturuyoruz ve baseColors'un elemanlarını ekliyoruz
        availableColors = new List<Color>(baseColors);
    }
}
