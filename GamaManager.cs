using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using DG.Tweening;

/*
-> Tanımlar
-> Start()  // Oyun başladığında çalışacak fonksiyon
-> ConfigureCells()  // Hücreleri ayarlayan fonksiyon
-> ChooseRandomFirstLetter()  // Rastgele bir harf seçen fonksiyon
-> moveCountUpdate()  // Hareket sayısını güncelleyen fonksiyon
-> OnCellClicked()  // Her hücreye tıklandığında çalışacak fonksiyon
-> OnCellHovered()  // Hücrelerin üzerine gelindiğinde çalışacak fonksiyon
-> OnLetterClicked()  // Her harfe tıklandığında çalışacak fonksiyon
-> OnLetterHovered()  // Harflerin üzerine gelindiğinde çalışacak fonksiyon
-> TurnBackLetter()  // Harf seçimi yanlışsa harfi eski haline döndüren fonksiyon
-> GameRule()  // Oyun kurallarını belirleyen fonksiyon
-> AddCelltoSelectedList()  // Doğru seçilmiş hücreleri kaydeden fonksiyon
-> AddCelltoSelected()  // Doğru seçilmiş hücreleri kaydeden fonksiyon
-> CellAnimation()  // Animasyonlar
-> isGameFinished()  // Oyunun bitip bitmediğini kontrol eden fonksiyon
-> GameEndRoutine()  // Oyun bittiğinde çalışacak rutin
-> GameEndAnimation()  // Oyun bittiğinde çalışacak animasyon
-> RandomColor()  // Rastgele renk seçen fonksiyon
-> FindWords()  // Kelimeleri bulan fonksiyon
-> controlWord()  // Kelimeleri kontrol eden fonksiyon
-> WordAnimation()  // Kelimeleri animasyonlayan fonksiyon
-> WordAnimationEnumerator()  // Kelimeleri animasyonlayan fonksiyon
*/

public class GamaManager : MonoBehaviour
{
    public ParticleSystemManager particleSystemManager;
    public JokerManager jokerManager;
    public GameEndAnimations gameEndAnimations;

    // Hücreleri tanımlar
    public GameObject[] cells;

    // Harfleri tanımlar
    public GameObject[] letters;

    // Alfabeyi tanımlar
    string[] alphabet = new string[] 
    { 
        "a","b","c","d","e","f","g","h","i","j","k",
        "l","m","n","o","p","q","r","s","t","u","v",
        "w","x","y","z"
    };

    // Hücrelerin içeriğini tutar
    private string[] cellData;

    private int cellCount = 0;

    private List<List<int>> wordIndices = new List<List<int>>();

    // Daha önce tamamlanmış kelimeleri saklayan liste
    private List<List<int>> completedWords = new List<List<int>>();


    // Seçilen hücrenin indexini tutar
    private int selectedCellIndex = -1;
    private List<int> selectedCellIndexes = new List<int>();
    private List<int> selectedLetterIndexes = new List<int>();

    // Seçilen harfin indexini tutar
    private int selectedLetterIndex = -1;


    // Hareket sayısını tutar
    private int moveCount = 10;
    public TMP_Text moveCountText;

    // Level sayısını tutar
    private int levelCount = 1;
    public TMP_Text levelCountText;

    // Kelime sayısını tutar
    private int wordCount = 0;
    public TMP_Text wordCountText;

    // Oyunun bitip bitmediğini kontrol eden değişkenler
    private bool isGameEnd = false;


    // Jokerler
    private bool isDictionaryJoker = false;
    private bool isPencilJoker = false;


    // Oyun sonu paneli
    public GameObject GameEndPanel;


    // 


    void Start()
    {
        // Her hücreye CellManager scripti ekle ve ayarlarını yap
        for (int i = 0; i < cells.Length; i++)
        {
            // Hücrede hali hazırda CellManager yoksa ekle
            CellManager cm = cells[i].GetComponent<CellManager>();
            if (cm == null) 
            {
                cm = cells[i].AddComponent<CellManager>();
            }
            cm.gameManager = this;  // Bu kısım önemli
            cm.cellIndex = i;
        }

        // Her harfe LetterManager scripti ekle ve ayarlarını yap
        for (int i = 0; i < letters.Length; i++)
        {
            // Harfte hali hazırda LetterManager yoksa ekle
            LetterManager lm = letters[i].GetComponent<LetterManager>();
            if (lm == null) 
            {
                lm = letters[i].AddComponent<LetterManager>();
            }
            lm.gameManager = this;  // Bu kısım önemli
            lm.letterIndex = i;
        }

        // Hücrelerin içeriğini belirle
        cellData = new string[] 
        {
            "0", "0", "0", "0", "0", "0", "0", "0",
            "0", "e", "l", "m", "a", "0", "0", "0",
            "0", "r", "0", "0", "0", "0", "0", "0",
            "l", "i", "m", "o", "n", "0", "0", "0",
            "0", "k", "0", "0", "a", "0", "0", "0",
            "0", "0", "0", "0", "n", "0", "0", "0",
            "0", "c", "i", "l", "e", "k", "0", "0",
            "0", "0", "0", "0", "0", "0", "0", "0",
        };

        levelCountText.text = levelCount.ToString();

        ConfigureCells(cellData, cells);

        FindWords(cellData, 8);

        moveCountUpdate(0);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameEndAnimations.CollectCoins(activeCells);
        }
    }

    // Aktif yaptığımız (SetActive(true)) hücreleri tutacak liste
    private List<GameObject> activeCells = new List<GameObject>();

    // Hücrelerimizi ayarlayan fonksiyon (CellManager'dan farklı bir isim kullandık)
    public void ConfigureCells(string[] cellData, GameObject[] cells)
    {
        // Her çağrıda listeyi temizle
        activeCells.Clear();

        for (int i = 0; i < cellData.Length; i++)
        {
            // Eğer cellData[i] alfabede varsa aktif yap ve içindeki TMP'ye yaz
            if (System.Array.Exists(alphabet, element => element == cellData[i]))
            {
                cells[i].SetActive(true);

                // Aktif ettiğimiz hücreyi listeye ekliyoruz
                activeCells.Add(cells[i]);

                cellCount++;
            }
            else
            {
                cells[i].SetActive(false);
            }
        }

        // Rastgele bir harf aç
        ChooseRandomFirstLetter();

        Debug.Log("ConfigureCells Complete");
    }


    // Rastgele bir harf seçen fonksiyon
    public void ChooseRandomFirstLetter()
    {
        // Hücrelerdeki farklı harfleri bul
        var uniqueCellData = cellData.Where(x => x != "0").Distinct().ToArray();

        int randomIndex = Random.Range(0, uniqueCellData.Length);
        OnCellClicked(System.Array.IndexOf(cellData, uniqueCellData[randomIndex]));
        OnLetterClicked(System.Array.IndexOf(alphabet, uniqueCellData[randomIndex]), true);
    }


    // Hareket sayısını güncelleyen fonksiyon
    public void moveCountUpdate(int count = -1){ 
        moveCount += count;
        
        moveCountText.text = moveCount.ToString();
    }

    // Her hücreye tıklandığında çalışacak fonksiyon
    public void OnCellClicked(int cellIndex)
    {
        if (isDictionaryJoker)
        {
            OnDictionaryJoker(cellIndex);
            isDictionaryJoker = false;
            return;
        }

        if (isPencilJoker)
        {
            OnPencilJoker(cellIndex);
            isPencilJoker = false;
            return;
        }

        if (selectedCellIndexes.Contains(cellIndex) || isGameEnd)
        {
            Debug.Log("You can't select this cell!");
            return;
        }

        if (selectedCellIndex != -1)
        {
            OnCellHovered(selectedCellIndex, Color.white, Color.black);
        }

        Debug.Log("Cell Clicked: " + cellIndex);
        OnCellHovered(cellIndex, new Color(1.0f, 0.8f, 0.0f), Color.white);

        selectedCellIndex = cellIndex;
    }
    

    // Hücrelerin üzerine gelindiğinde çalışacak fonksiyon
    public void OnCellHovered(int cellIndex, Color BGColor, Color TextColor)
    {
        cells[cellIndex].GetComponent<Image>().color = BGColor;
        cells[cellIndex].GetComponentInChildren<TextMeshProUGUI>().color = TextColor;
    }


    // Her harfe tıklandığında çalışacak fonksiyon
    public void OnLetterClicked(int letterIndex, bool isJoker = false)
    {
        if (selectedLetterIndexes.Contains(letterIndex) || isGameEnd)
        {
            Debug.Log("You can't select this letter!");
            return;
        }
        
        if (selectedCellIndex != -1)
        {
            if (selectedLetterIndex != -1)
            {
                OnLetterHovered(selectedLetterIndex, Color.white, Color.black);
            }

            OnLetterHovered(letterIndex, Color.red, Color.white);
            Debug.Log("Letter Clicked: " + letterIndex);
            selectedLetterIndex = letterIndex;

            GameRule(letterIndex, selectedCellIndex);

            if(!isJoker){
                moveCountUpdate();
            }

            isGameFinished();
           
        }
        else
        {
            Debug.Log("Please select a cell first!");
        }   
    }


    // Harflerin üzerine gelindiğinde çalışacak fonksiyon
    public void OnLetterHovered(int letterIndex, Color BGColor, Color TextColor)
    {
        letters[letterIndex].GetComponent<Image>().color = BGColor;
        letters[letterIndex].GetComponentInChildren<TextMeshProUGUI>().color = TextColor;
    }


    // Harf seçimi yanlışsa harfi eski haline döndüren fonksiyon
    private IEnumerator TurnBackLetter(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        OnLetterHovered(selectedLetterIndex, Color.white, Color.black);
    }


    // Oyun kurallarını belirleyen fonksiyon
    public void GameRule(int letterIndex, int cellIndex)
    {
        string selectedCellLetter = cellData[cellIndex];
        string selectedLetter = alphabet[letterIndex];

        // Eğer seçilen harf ve hücredeki harf aynıysa doğru cevap verilmiştir
        if (selectedCellLetter == selectedLetter)
        {
            AddCelltoSelectedList(selectedCellLetter, cellIndex);
            
            selectedLetterIndexes.Add(letterIndex);
            OnLetterHovered(letterIndex, Color.green, Color.white);

            Debug.Log("Correct!");

            selectedCellIndex = -1;
            selectedLetterIndex = -1;
        }
        else
        {
            Debug.Log("Wrong!");
            StartCoroutine(TurnBackLetter(1.0f));
        }
    }

    public void AddCelltoSelectedList(string selectedCellLetter, int firstCellIndex){
        StartCoroutine(AddCelltoSelected(selectedCellLetter, firstCellIndex));
    }

    // Doğru seçilmiş hücreleri kaydeden fonksiyon
    public IEnumerator AddCelltoSelected(string selectedCellLetter, int firstCellIndex)
    {
        List<int> theCells = new List<int>();

        selectedCellIndexes.Add(firstCellIndex);
        theCells.Add(firstCellIndex);
        CellAnimation(cells[firstCellIndex], firstCellIndex, Color.green, Color.white);

        for (int i = 0; i < cellData.Length; i++)
        {
            if (cellData[i] == selectedCellLetter && i != firstCellIndex)
            {
                selectedCellIndexes.Add(i);
                theCells.Add(i);
            }
        }

        for (int i = 0; i < theCells.Count; i++)
        {
            CellAnimation(cells[theCells[i]], theCells[i], Color.green, Color.white);

            TextMeshProUGUI textComp = cells[theCells[i]].GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = cellData[theCells[i]].ToUpper();
            }

            yield return new WaitForSeconds(0.4f);
        }
    }


    // Animasyonlar
    public void CellAnimation(GameObject cell, int cellIndex, Color BGColor, Color TextColor)
    {
        // Canvas bileşenini al veya ekle
        Canvas canvas = cell.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = cell.AddComponent<Canvas>();
        }

        // Render Mode ve Sorting Order ayarları
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10; // En üstte görünmesi için yüksek bir değer

        // Renkleri ayarla
        OnCellHovered(cellIndex, BGColor, TextColor);

        // Animasyonu başlat
        Sequence sequence = DOTween.Sequence();
        sequence.Append(cell.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.3f).SetEase(Ease.OutBounce))
                .Append(cell.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad))
                .OnComplete(() =>
                {
                    // Animasyon tamamlandıktan sonra sıralamayı sıfırla
                    canvas.sortingOrder = 0;
                    canvas.overrideSorting = false;
                });
    }



    // Oyunun bitip bitmediğini kontrol eden fonksiyon
    public void isGameFinished()
    {
        if (selectedCellIndexes.Count == cellCount)
        {
            GameEndRoutine(true);
            isGameEnd = true;
        }
        else if(moveCount == 0){
            GameEndRoutine(false);
            isGameEnd = true;
        }
        else
        {
            controlWord();
        }
    }

    // Oyun bittiğinde çalışacak rutin
    public void GameEndRoutine(bool isGameSuccess)
    {
        Debug.Log("Oyun Bitti!");
        StartCoroutine(GameEndAnimation(isGameSuccess));
    }

    // Oyun bittiğinde çalışacak fonksiyon
    public IEnumerator GameEndAnimation(bool isGameSuccess)
    {   
        yield return new WaitForSeconds(1.0f);

        if(isGameSuccess){
            for (int i = 0; i < selectedCellIndexes.Count; i++)
            {
                CellAnimation(cells[selectedCellIndexes[i]], selectedCellIndexes[i], RandomColor(), Color.white);
                yield return new WaitForSeconds(0.05f);
            }
            /*for (int i = 0; i < selectedLetterIndexes.Count; i++)
            {
                CellAnimation(letters[selectedLetterIndexes[i]], selectedLetterIndexes[i], RandomColor(), Color.white);
                yield return new WaitForSeconds(0.05f);
            }*/
        }
        else{
            for (int i = 0; i < selectedCellIndexes.Count; i++)
            {
                CellAnimation(cells[selectedCellIndexes[i]], selectedCellIndexes[i], Color.red, Color.white);

                yield return new WaitForSeconds(0.05f);
            }
        }

        yield return new WaitForSeconds(2.0f);
        GameEndPanel.SetActive(true);

        GameEndPanelController(isGameSuccess);
        
    }

    public GameObject congratulationText;
    public void GameEndPanelController(bool isGameSuccess){
        if(isGameSuccess){
            StartCoroutine(GameEndSuccessRoutine());
        }
    }

    public IEnumerator GameEndSuccessRoutine(){
        yield return new WaitForSeconds(0.5f);
        congratulationText.SetActive(true);
        congratulationText.transform.localScale = Vector3.zero;
        congratulationText.transform.DOScale(1.5f, 1.0f).SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(0.5f);
        FireWorkCaller();
    }

    public void FireWorkCaller(){
        StartCoroutine(FireWorkRoutine());
    }

    public IEnumerator FireWorkRoutine(){
        for (int i = 0; i < 5; i++)
        {
            float randomX = Random.Range(-2f, 2f);
            float randomY = Random.Range(-2f, 2f);

            Vector3 randomPos = new Vector3(randomX, randomY, 0);

            particleSystemManager.PlayFirework(randomPos);
            yield return new WaitForSeconds(0.5f);
        }
    }


    public void GameRestart(){
        GameEndPanel.SetActive(false);
        isGameEnd = false;
        selectedCellIndexes.Clear();
        selectedLetterIndexes.Clear();
        completedWords.Clear();
        selectedCellIndex = -1;
        selectedLetterIndex = -1;
        moveCount = 10;
        levelCount++;
        levelCountText.text = levelCount.ToString();
        ConfigureCells(cellData, cells);
        FindWords(cellData, 8);
        moveCountUpdate(0);
    }


    private Color[] baseColors = new Color[]
    {
        new Color(1.0f, 0.0f, 0.0f),   // CANLI KIRMIZI
        new Color(0.0f, 0.0f, 1.0f),   // PARLAK MAVİ
        new Color(1.0f, 0.0f, 1.0f),   // NEON PEMBE (Magenta)
        new Color(1.0f, 0.3f, 0.0f),   // MERCAN TURUNCU
        new Color(0.0f, 1.0f, 1.0f),   // CANLI CAMGÖBEĞİ (Cyan)
        new Color(1.0f, 1.0f, 0.0f),   // FOSFORLU SARI
        new Color(1.0f, 0.85f, 0.0f),  // ALTIN SARISI (daha parlak)
        new Color(0.7f, 0.0f, 1.0f),   // ELEKTRİK MORU
        new Color(0.0f, 0.9f, 1.0f),   // PARLAK TURKUAZ
        new Color(1.0f, 0.35f, 0.2f),  // CANLI MERCAN
        new Color(0.0f, 1.0f, 0.5f)    // DENİZ YEŞİLİ (daha floresan)
    };

    private List<Color> availableColors;

    private void Awake()
    {
        ResetColorList();
    }

    public Color RandomColor()
    {
        if (availableColors.Count == 0)
        {
            Debug.LogWarning("Tüm renkler kullanıldı! Liste sıfırlanıyor...");
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



    public void controlWord()
    {
        foreach (var wordIndexes in wordIndices)
        {
            // Eğer kelime daha önce tamamlanmışsa tekrar işlem yapma
            if (completedWords.Any(completedWord => completedWord.SequenceEqual(wordIndexes)))
            {
                continue;
            }

            // Eğer tüm harf indeksleri selectedCellIndexes içinde varsa kelime tamamlanmıştır
            if (wordIndexes.All(index => selectedCellIndexes.Contains(index)))
            {
                completedWords.Add(new List<int>(wordIndexes)); // Tamamlanan kelimeyi listeye ekle
                WordAnimation(wordIndexes);
            }
        }
    }

    public void WordAnimation(List<int> wordIndexes)
    {
        StartCoroutine(WordAnimationEnumerator(wordIndexes));
    }

    public IEnumerator WordAnimationEnumerator(List<int> wordIndexes)
    {
        Color BGColor = RandomColor();

        yield return new WaitForSeconds(1.0f);
        for (int i = 0; i < wordIndexes.Count; i++)
        {
            Transform currentCell = cells[wordIndexes[i]].transform; 
            
            CellAnimation(cells[wordIndexes[i]], wordIndexes[i], BGColor, Color.white);
            //particleSystemManager.PlayCellEffect(currentCell.position);
            yield return new WaitForSeconds(0.1f);
        }
    }


    //Jokerler
    // Pencil  --> Seçilen hücredeki harfi açar
    // Dice  --> Rastgele bir harf seçer
    // Dictionary  --> Kelimeyi bulur ve bütün harflerini açar
    
    public void OnPencilJokerClicked()
    {
        if (selectedCellIndex != -1)
        {
            // Seçili hücreyi iptal et
            OnCellHovered(selectedCellIndex, Color.white, Color.black);
            selectedCellIndex = -1;
        }

        // Joker zaten aktifse kapat, değilse aç
        isPencilJoker = !isPencilJoker;
    }

    public void OnPencilJoker(int cellIndex){
        StartCoroutine(OnPencilJokerRoutine(cellIndex));
    }

    public IEnumerator OnPencilJokerRoutine(int cellIndex)
    {
        selectedCellIndex = cellIndex;
        string selectedCellValue = cellData[selectedCellIndex];

        // Eğer seçilen hücre boş değilse işlemi gerçekleştir
        if (selectedCellValue != "0")
        {
            jokerManager.UsePencilJoker(new List<GameObject> {cells[selectedCellIndex]});

            yield return new WaitForSeconds(0.7f);
            OnLetterClicked(System.Array.IndexOf(alphabet, selectedCellValue), true);
             
            jokerManager.StopJokerAnimation(FindObjectOfType<JokerManager>().pencilJoker);
        } 
    }


    public void OnDiceJokerClicked(){
        StartCoroutine(DiceJokerRoutine());
        isDictionaryJoker = false;
        isPencilJoker = false;
    }


    public IEnumerator DiceJokerRoutine(){
        yield return new WaitForSeconds(0.5f);
        // Hücrelerdeki farklı harfleri bul
        var uniqueCellData = cellData.Where(x => x != "0").Distinct().ToArray();
        var uniqueCellIndex = uniqueCellData.Except(selectedCellIndexes.Select(x => cellData[x])).ToArray();

        int randomIndex = Random.Range(0, uniqueCellIndex.Length);
        OnCellClicked(System.Array.IndexOf(cellData, uniqueCellIndex[randomIndex]));
        OnLetterClicked(System.Array.IndexOf(alphabet, uniqueCellIndex[randomIndex]), true);
    }


    public void OnDictionaryJokerClicked(){
        if(selectedCellIndex != -1){
            OnCellHovered(selectedCellIndex, Color.white, Color.black);
            selectedCellIndex = -1;
        }
        isDictionaryJoker = !isDictionaryJoker;
    }


    public void OnDictionaryJoker(int cellIndex){
        selectedCellIndex = cellIndex;
        OnCellHovered(selectedCellIndex, new Color(1.0f, 0.8f, 0.0f), Color.white);

        for (int i = 0; i < wordIndices.Count; i++)
        {
            if (wordIndices[i].Contains(selectedCellIndex))
            {
                StartCoroutine(DictionaryJokerRoutine(wordIndices[i]));
            }
        }

        jokerManager.StopJokerAnimation(FindObjectOfType<JokerManager>().dictionaryJoker);
    }


    public IEnumerator DictionaryJokerRoutine(List<int> wordIndexes)
    {
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < wordIndexes.Count; i++)
        {
            OnCellClicked(wordIndexes[i]);
            OnLetterClicked(System.Array.IndexOf(alphabet, cellData[wordIndexes[i]]), true);
            yield return new WaitForSeconds(0.05f);
        }
    }


    // Joker Animasyonları

    
    
}
