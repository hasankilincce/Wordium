using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Globalization;

public class JokerManager : MonoBehaviour
{
    public CellAnimations cellAnimations;
    public JokerAnimations jokerAnimations;
    public WordAnimation wordAnimation;
    public LetterAnimations letterAnimations;
    public UserDataManager userDataManager;
    public GameUIManager gameUIManager;

    public GameManager gameManager;
    public RandomColor randomColor;

    public bool isDictionaryJoker = false;
    public bool isPencilJoker = false;

    public int selectedCellIndex;


    //Jokerler
    // Pencil  --> Seçilen hücredeki harfi açar
    // Dice  --> Rastgele bir harf seçer
    // Dictionary  --> Kelimeyi bulur ve bütün harflerini açar
    
    public void OnPencilJokerClicked()
    {
        selectedCellIndex = gameManager.selectedCellIndex;

        if (selectedCellIndex != -1)
        {
            // Seçili hücreyi iptal et
            cellAnimations.OnCellHovered(gameManager.cells[selectedCellIndex], gameManager.Black, Color.white);
            //gameManager.selectedCellIndex = -1;
        }

        // Joker zaten aktifse kapat, değilse aç
        isPencilJoker = !isPencilJoker;
        isDictionaryJoker = false;
    }

    public void OnPencilJoker(){
        StartCoroutine(OnPencilJokerRoutine());
    }

    public IEnumerator OnPencilJokerRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        
        string selectedLetter = gameManager.NormalizeString(gameManager.GetCellData()[gameManager.selectedCellIndex]).ToLower(new CultureInfo("tr-TR"));
        int letterIndex = System.Array.IndexOf(gameManager.alphabet, selectedLetter);


        gameManager.OnLetterClicked(letterIndex, true);
            
        jokerAnimations.StopJokerAnimation(FindObjectOfType<JokerAnimations>().pencilJoker);


        ChangeJokerCount(1, -1);
        gameUIManager.UpdateJokerCountTexts();
    }


    public void OnDiceJokerClicked(){
        StartCoroutine(DiceJokerRoutine(gameManager.GetCellData()));
        isDictionaryJoker = false;
        isPencilJoker = false;
    }


    public IEnumerator DiceJokerRoutine(string[] cellData){
        yield return new WaitForSeconds(0.5f);
        // Hücrelerdeki farklı harfleri bul
        var uniqueCellData = cellData.Where(x => x != "0").Distinct().ToArray();
        var uniqueCellIndex = uniqueCellData.Except(gameManager.selectedCellIndexes.Select(x => cellData[x])).ToArray();

        int randomIndex = Random.Range(0, uniqueCellIndex.Length);
        gameManager.OnCellClicked(System.Array.IndexOf(cellData, uniqueCellIndex[randomIndex]));
        gameManager.OnLetterClicked(System.Array.IndexOf(gameManager.alphabet, uniqueCellIndex[randomIndex]), true);
    }


    public void OnDictionaryJokerClicked(){
        selectedCellIndex = gameManager.selectedCellIndex;

        if(selectedCellIndex != -1){
            cellAnimations.OnCellHovered(gameManager.cells[selectedCellIndex], gameManager.Black, Color.white);
            gameManager.selectedCellIndex = -1;
        }
        isDictionaryJoker = !isDictionaryJoker;
        isPencilJoker = false;
    }


    public void OnDictionaryJoker(int cellIndex){
        List<List<int>> wordIndices = wordAnimation.wordIndices;

        selectedCellIndex = cellIndex;
        cellAnimations.OnCellHovered(gameManager.cells[selectedCellIndex], new Color(1.0f, 0.8f, 0.0f), Color.white);

        for (int i = 0; i < wordIndices.Count; i++)
        {
            if (wordIndices[i].Contains(selectedCellIndex))
            {
                StartCoroutine(DictionaryJokerRoutine(wordIndices[i], gameManager.GetCellData()));
                break;
            }
        }

        jokerAnimations.StopJokerAnimation(FindObjectOfType<JokerAnimations>().dictionaryJoker);
    }


    public IEnumerator DictionaryJokerRoutine(List<int> wordIndexes, string[] cellData)
    {
        ChangeJokerCount(2, -1);
        gameUIManager.UpdateJokerCountTexts();

        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < wordIndexes.Count; i++)
        {
            gameManager.OnCellClicked(wordIndexes[i]);
            gameManager.OnLetterClicked(System.Array.IndexOf(gameManager.alphabet, cellData[wordIndexes[i]]), true);
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void OnBrushJokerClicked(){
        List<int> notUsedIndexes = gameManager.GetNotUsedLetters();
        StartCoroutine(BrushJokerRoutine(notUsedIndexes));
        isDictionaryJoker = false;
        isPencilJoker = false;

        gameManager.isBrushJokerUsed = true;
    }


    
    public IEnumerator BrushJokerRoutine(List<int> notUsedIndexes){
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < notUsedIndexes.Count; i++)
        {
            gameManager.selectedLetterIndexes.Add(notUsedIndexes[i]);
            letterAnimations.OnLetterHovered(gameManager.letters[notUsedIndexes[i]], Color.gray, Color.white);
            yield return new WaitForSeconds(0.05f);
        }
    }
    

    public bool IsUserJokerEnough(int whichJoker)
    {
        // Mevcut joker sayımlarını bir diziye atayalım
        int[] jokerCounts = {
            PlayerPrefs.GetInt("diceJoker"),
            PlayerPrefs.GetInt("pencilJoker"),
            PlayerPrefs.GetInt("dictionaryJoker"),
            PlayerPrefs.GetInt("brushJoker") };

        // Joker isimlerini de dizi hâlinde tutarsak hata mesajını dinamik oluşturabiliriz
        string[] jokerNames = { "Dice", "Pencil", "Dictionary", "Brush" };
        
        // Gönderilen joker index'i geçerli mi kontrol edelim
        if (whichJoker < 0 || whichJoker >= jokerCounts.Length)
        {
            // Geçersiz joker numarası
            Debug.Log("Geçersiz joker indeksi!");
            return false;
        }

        // İlgili joker sayımı 0'dan büyük mü kontrol edelim
        if (jokerCounts[whichJoker] > 0)
        {
            return true;
        }
        else
        {
            Debug.Log($"{jokerNames[whichJoker]} Joker Yok!");
            return false;
        }
    }



    public void ChangeJokerCount(int whichJoker, int changeValue){
        switch (whichJoker)
        {
            case 0:
                PlayerPrefs.SetInt("diceJoker", PlayerPrefs.GetInt("diceJoker") + changeValue);
                break;
            case 1:
                PlayerPrefs.SetInt("pencilJoker", PlayerPrefs.GetInt("pencilJoker") + changeValue);
                break;
            case 2:
                PlayerPrefs.SetInt("dictionaryJoker", PlayerPrefs.GetInt("dictionaryJoker") + changeValue);
                break;
            case 3:
                PlayerPrefs.SetInt("brushJoker", PlayerPrefs.GetInt("brushJoker") + changeValue);
                break;
        };
    }
}