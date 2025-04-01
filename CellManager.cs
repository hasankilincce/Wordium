using UnityEngine;
using UnityEngine.UI; // <-- Button için gerekli
using System.Collections;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    public GameManager gameManager;
    public int cellIndex;

    private Button button;

    private void Awake()
    {
        InitilazeButton();
    }

    public void InitilazeButton(){
        // Aynı GameObject üzerinde bulunan Button komponentine eriş
        button = GetComponent<Button>();

        // Button bulunursa, onClick event'ine listener ekle
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            //Debug.LogError($"{name} üzerinde Button komponenti bulunamadı!");
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log($"Cell clicked via Button: {cellIndex}");

        if (gameManager != null)
        {
            gameManager.OnCellClicked(cellIndex);
        }
    }
}
