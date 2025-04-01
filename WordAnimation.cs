using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WordAnimation : MonoBehaviour
{
    public CellAnimations cellAnimations;
    public RandomColor randomColor;

    public List<List<int>> wordIndices = new List<List<int>>();
    // Daha önce tamamlanmış kelimeleri saklayan liste
    private List<List<int>> completedWords = new List<List<int>>();
    public TMP_Text wordCountText;

    // Kelimeleri bulan fonksiyon
    public void FindWords(string[] grid, int size = 8)
    {
        List<string> words = new List<string>();
        HashSet<int> usedIndices = new HashSet<int>();
        int wordCount = 0;

        // Satırlarda kelime bul
        for (int row = 0; row < size; row++)
        {
            string currentWord = "";
            List<int> currentIndices = new List<int>();

            for (int col = 0; col < size; col++)
            {
                int index = row * size + col;
                string letter = grid[index];

                if (letter != "0")
                {
                    currentWord += letter;
                    currentIndices.Add(index);
                }
                else
                {
                    if (currentWord.Length > 1)
                    {
                        words.Add(currentWord);
                        wordIndices.Add(new List<int>(currentIndices)); // İndeksleri ekle
                        usedIndices.UnionWith(currentIndices);
                        wordCount++;
                    }
                    currentWord = "";
                    currentIndices.Clear();
                }
            }
            if (currentWord.Length > 1)
            {
                words.Add(currentWord);
                wordIndices.Add(new List<int>(currentIndices)); // İndeksleri ekle
                usedIndices.UnionWith(currentIndices);
                wordCount++;
            }
        }

        // Sütunlarda kelime bul
        for (int col = 0; col < size; col++)
        {
            string currentWord = "";
            List<int> currentIndices = new List<int>();

            for (int row = 0; row < size; row++)
            {
                int index = row * size + col;
                string letter = grid[index];

                if (letter != "0")
                {
                    currentWord += letter;
                    currentIndices.Add(index);
                }
                else
                {
                    if (currentWord.Length > 1)
                    {
                        words.Add(currentWord);
                        wordIndices.Add(new List<int>(currentIndices)); // İndeksleri ekle
                        usedIndices.UnionWith(currentIndices);
                        wordCount++;
                    }
                    currentWord = "";
                    currentIndices.Clear();
                }
            }
            if (currentWord.Length > 1)
            {
                words.Add(currentWord);
                wordIndices.Add(new List<int>(currentIndices)); // İndeksleri ekle
                usedIndices.UnionWith(currentIndices);
                wordCount++;
            }
        }

        wordCountText.text = wordCount.ToString();

        // Sonuçları yazdır
        foreach (var indices in wordIndices)
        {
            Debug.Log("Word Indices: [" + string.Join(",", indices) + "]");
        }
    }



    public void controlWord(List<int> selectedCellIndexes, GameObject[] cells)
    {
        Debug.Log("controlWord() çalıştı.");
        foreach (var wordIndexes in wordIndices)
        {
            Debug.Log($"Kelime Kontrol Ediliyor: {string.Join(",", wordIndexes)}");

            if (completedWords.Any(completedWord => completedWord.SequenceEqual(wordIndexes)))
            {
                Debug.Log("Bu kelime zaten tamamlanmış.");
                continue;
            }

            if (wordIndexes.All(index => selectedCellIndexes.Contains(index)))
            {
                Debug.Log("Kelime tamamlandı, animasyon başlıyor.");
                completedWords.Add(new List<int>(wordIndexes));
                AnimateWord(wordIndexes, cells);
            }
        }
    }


    public void AnimateWord(List<int> wordIndexes, GameObject[] cells)
    {
        Debug.Log("AnimateWord() çağrıldı. Animasyon başlıyor.");
        StartCoroutine(WordAnimationEnumerator(wordIndexes, cells));
    }


    public IEnumerator WordAnimationEnumerator(List<int> wordIndexes, GameObject[] cells)
    {
        Debug.Log("WordAnimationEnumerator başladı.");
        Color BGColor = randomColor.GetRandomColor();
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < wordIndexes.Count; i++)
        {
            Debug.Log($"Hücre Animasyonu: {wordIndexes[i]}");

            Transform currentCell = cells[wordIndexes[i]].transform; 
            cellAnimations.CellAnimation(cells[wordIndexes[i]], BGColor, Color.white);

            yield return new WaitForSeconds(0.1f);
        }
    }


    public void ClearWordLists(){
        wordIndices.Clear();
        completedWords.Clear();
    }

}
