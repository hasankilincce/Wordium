using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterManager : MonoBehaviour, IPointerClickHandler
{
    // Burada GamaManager olarak tanımlıyoruz (GameManager değil!)
    public GameManager gameManager;
    public int letterIndex;

    // IPointerClickHandler arayüzü metodu
    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameManager != null)
        {
            // Hücrenin tıklandığını GamaManager'a iletir
            gameManager.OnLetterClicked(letterIndex);
        }
    }
}
