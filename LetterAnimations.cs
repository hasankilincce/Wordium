using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterAnimations : MonoBehaviour
{
    // Harflerin üzerine gelindiğinde çalışacak fonksiyon
    public void OnLetterHovered(GameObject letter, Color BGColor, Color TextColor)
    {
        letter.GetComponent<Image>().color = BGColor;
        letter.GetComponentInChildren<TextMeshProUGUI>().color = TextColor;
    }


    // Harf seçimi yanlışsa harfi eski haline döndüren fonksiyon
    public IEnumerator TurnBackLetter(float waitTime, GameObject letter)
    {
        yield return new WaitForSeconds(waitTime);
        OnLetterHovered(letter, Color.white, Color.black);
    }
}
