using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using System.Text;
using System.Globalization;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public CellIndexer cellIndexer;
    public FirebaseAnalyticsManager firebaseAnalyticsManager;
    public LocalDataManager localDataManager;
    public ParticleSystemManager particleSystemManager;
    public JokerManager jokerManager;
    public UserDataManager userDataManager;
    public StrikeManager strikeManager;
    public SoundManager soundManager;
    public VibrationManager vibrationManager;
    public CoinManager coinManager;
    public AdManager adManager;
    public CellAnimations cellAnimations;
    public LetterAnimations letterAnimations;
    public WordAnimation wordAnimation;
    public JokerAnimations jokerAnimations;
    public GameEndUI gameEndUI;
    public GameUIManager gameUIManager;
    public PopUpUI popUpUI;
    public RandomColor randomColor;


    [Tooltip("Kullanılabilir harfler.")]
    public string[] alphabet = new string[]
    {
        "a", "b", "c", "ç", "d", "e", "f", "g", "ğ", "h", "ı", "i", 
        "j", "k", "l", "m", "n", "o", "ö", "p", "r", "s", "ş", "t", 
        "u", "ü", "v", "y", "z"
    };



    [Header("Cell Objects")]
    public GameObject[] cells;
    public GameObject[] letters;
    private int cellCount = 0;


    private int moveCount;

    private string[] cellData;
    
    // Seçilen hücrenin indexini tutar
    public int selectedCellIndex = -1;
    private int selectedLetterIndex = -1;

    public List<int> gameCellIndexes = new List<int>();
    public List<int> selectedCellIndexes = new List<int>();
    public List<int> selectedCellIndexesFromPlayer = new List<int>();
    public List<int> selectedLetterIndexes = new List<int>();
            // 2) Alfabede olup da usedLetters içinde olmayanları bulalım
    private List<string> notUsedLetters = new List<string>();
    private List<int> notUsedIndexes = new List<int>();

    // Joker
    public bool isBrushJokerUsed = false;

    // Oyunun bitip bitmediğini kontrol eden değişkenler
    private bool isGameEnd = false;
    private bool isExtraMoveTaked = false;


    public TMP_Text moveCountText;
    public TMP_Text levelTitleText;
    public TMP_Text levelCategoryText;

    private int level;
    private string category;

    public GameObject gameUIPanel;
    public GameObject gameAreaPanel;


    //COLORS
    Color Green = new Color(0.0235f, 0.8157f, 0.0039f);
    public Color Black = new Color(0.1333333f, 0.1333333f, 0.1333333f);


    // Tasks
    public int noJokerGames;
    public int noWrongGames;


    public void GameStart(int userLevel){
        gameEndUI.CloseGameEndPanel();
        //gameUIPanel.SetActive(true);

        GameStartRoutine(userLevel);

        levelTitleText.text = level.ToString() + ". SEVİYE";
        levelCategoryText.text = category.ToUpper();

        gameUIManager.UpdateJokerCountTexts();

        gameAreaPanel.SetActive(true);

        strikeManager.ApplayStrikeRoutine();

        firebaseAnalyticsManager.LogLevelStart(level);

        noJokerGames = 0;
        noWrongGames = 0;
    }

    private void GameStartRoutine(int userLevel)
    {
        LoadLevelCellData(userLevel);
        
        ConfigureCells(cellData, cells);

        //Debug.Log("=== Alphabet Listesi ===");
        foreach (var letter in alphabet)
        {
            //Debug.Log($"Alphabet Harf: {letter} | Unicode Kodu: {(int)letter[0]} | Normalized: {letter.Normalize(NormalizationForm.FormC)}");
        }

        //Debug.Log("=== Firestore'dan Gelen cellData ===");
        foreach (var cell in cellData)
        {
            string normalizedCell = cell.Trim().Normalize(NormalizationForm.FormC);
            //Debug.Log($"cellData Harf: {cell} | Unicode Kodu: {(int)cell[0]} | Normalized: {normalizedCell}");
        }

        wordAnimation.FindWords(cellData, 8);

        ListUnusedAlphabetCharsWithIndex();


        /*if (gameCellIndexes.Count > 0)
        {
            // Harflerin kendisi
            Debug.Log("cellData'da kullanılan hücreler: "
                      + string.Join(", ", gameCellIndexes));
        }*/
    }

   public void GameRestart(int userLevel)
    {
        // Eğer o anda bir coroutine varsa iptal edelim
        StopAllCoroutines();

        popUpUI.CloseFullHealthPanel();

        // Oyun bitiş durumunu sıfırla
        isGameEnd = false;
        isExtraMoveTaked = false;

        // Seçilmiş hücre ve harf indexlerini sıfırla
        selectedCellIndex = -1;
        selectedLetterIndex = -1;

        selectedCellIndexes.Clear();
        selectedCellIndexesFromPlayer.Clear();
        selectedLetterIndexes.Clear();

        // Kullanılmayan harf ve index listelerini de sıfırlayalım
        notUsedLetters.Clear();
        notUsedIndexes.Clear();
        gameCellIndexes.Clear();

        //Word listleri temizle
        wordAnimation.ClearWordLists();

        // Hücre sayısı ve hareket sayısını sıfırla
        cellCount = 0;
        moveCount = 0;

        // UI resetlemek isterseniz:
        moveCountText.text = "0";

        // Tüm hücreleri devre dışı (veya varsayılan görsel durumuna) getir
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].SetActive(true);
            cells[i].GetComponent<CellManager>().InitilazeButton();
            // Hücre rengi vb. de sıfırlanabilir
            cells[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
            cellAnimations.OnCellHovered(cells[i], Black, Color.white);
        }

        for (int i = 0; i < letters.Length; i++){
            letterAnimations.OnLetterHovered(letters[i], Color.white, Black);
        }

        // Oyun bitti panelini kapat
        gameEndUI.CloseGameEndPanel();

        // Paneli tekrar aktif hâle getirelim
        ShowHideGameUIPanel(true);

        // Yeniden başlat
        GameStart(userLevel);
    }

    public void ShowHideGameUIPanel(bool isShow){
        gameUIPanel.SetActive(isShow);
    }

    // Hareket sayısını güncelleyen fonksiyon
    public void moveCountUpdate(int count = -1){ 
        moveCount += count;
        
        moveCountText.text = moveCount.ToString();
    }


    internal void OnCellClicked(int cellIndex)
    {
        if (jokerManager.isDictionaryJoker)
        {
            jokerManager.OnDictionaryJoker(cellIndex);
            jokerManager.isDictionaryJoker = false;
            return;
        }

        if (jokerManager.isPencilJoker)
        {
            selectedCellIndex = cellIndex;

            jokerManager.OnPencilJoker();
            jokerManager.isPencilJoker = false;

            return;
        }


        if (selectedCellIndexes.Contains(cellIndex) || isGameEnd)
        {
            Debug.Log("You can't select this cell!");
            return;
        }

        if (selectedCellIndex != -1)
        {
            cellAnimations.OnCellHovered(cells[selectedCellIndex], Black, Color.white);
        }

        Debug.Log("Cell Clicked: " + cellIndex);
        cellAnimations.OnCellHovered(cells[cellIndex] ,new Color(1.0f, 0.8f, 0.0f), Color.white);

        selectedCellIndex = cellIndex;
    }

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
                letterAnimations.OnLetterHovered(letters[selectedLetterIndex], Color.white, Color.black);
            }

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


    // Oyun kurallarını belirleyen fonksiyon
    public void GameRule(int letterIndex, int cellIndex)
    {
        string selectedCellLetter = NormalizeString(cellData[cellIndex]).ToLower(new CultureInfo("tr-TR"));
        string selectedLetter = NormalizeString(alphabet[letterIndex]).ToLower(new CultureInfo("tr-TR"));

        Debug.Log($"Karşılaştırma: Hücre Harfi = '{selectedCellLetter}' | Seçilen Harf = '{selectedLetter}'");

        if (selectedCellLetter == selectedLetter)
        {
            AddCelltoSelectedList(selectedCellLetter, cellIndex);
            selectedLetterIndexes.Add(letterIndex);
            letterAnimations.OnLetterHovered(letters[selectedLetterIndex], Green, Color.white);
            Debug.Log("Correct!");

            soundManager.PlayCorrectSound();
            
            SelectAnotherCell();
            
            selectedLetterIndex = -1;
        }
        else
        {
            letterAnimations.OnLetterHovered(letters[letterIndex], Color.red, Color.white);
            Debug.Log("Wrong!");

            vibrationManager.WrongVibrate();

            soundManager.PlayWrongSound();
            
            StartCoroutine(letterAnimations.TurnBackLetter(1.0f, letters[selectedLetterIndex]));

            noWrongGames++;
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
        selectedCellIndexesFromPlayer.Add(firstCellIndex);
        theCells.Add(firstCellIndex);
        cellAnimations.CellAnimation(cells[firstCellIndex], Green, Color.white);

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
            cellAnimations.CellAnimation(cells[theCells[i]], Green, Color.white);

            TextMeshProUGUI textComp = cells[theCells[i]].GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = cellData[theCells[i]].ToUpper();
            }

            yield return new WaitForSeconds(0.3f);
        }
    }


    private void LoadLevelCellData(int userLevel)
    {
        List<LevelData> loadedLevels = localDataManager.LoadLocalLevelData();
        LevelData levelData = loadedLevels.FirstOrDefault(d => d.level == userLevel.ToString());

        if (levelData == null)
        {
            Debug.LogWarning($"Level {userLevel} için data bulunamadı!");
            // Buraya uyarı popup'ı ekle
            return;
        }

        Debug.Log($"Level: {levelData.level}, Category: {levelData.category}, MoveCount: {levelData.moveCount}");
        cellData = levelData.cellData.Select(c => c.ToString()).ToArray();

        moveCount = levelData.moveCount;
        level = Convert.ToInt32(levelData.level);
        category = levelData.category;
        
        moveCountUpdate(0);

    }

    public string NormalizeString(string input)
    {
        return input.Trim().Normalize(NormalizationForm.FormC);
    }


    public void isGameFinished()
    {
        Debug.Log("isGameFinished() çağrıldı.");

        if (selectedCellIndexes.Count == cellCount)
        {
            GameEndRoutine(true);
            isGameEnd = true;

            adManager.ShowInterstitialAdWithRoutine();

            firebaseAnalyticsManager.LogLevelComplete(level);
        }
        else if(moveCount == 0){
            if (isExtraMoveTaked)
            {
                GameEndRoutine(false);
                isGameEnd = true;

                firebaseAnalyticsManager.LogLevelFail(level);
            }else
            {
                ExtraMoveRoutine();
            }
            
        }
        else
        {
            wordAnimation.controlWord(selectedCellIndexes, cells);
        }
    }

    private void ExtraMoveRoutine()
    {
        Debug.Log("ExtraMoveRoutine() çağrıldı.");

        popUpUI.ShowExtraMovePanel();
        isExtraMoveTaked = true;
    }


    // Oyun bittiğinde çalışacak rutin
    public void GameEndRoutine(bool isGameSuccess)
    {
        Debug.Log("Oyun Bitti!");
        ShowHideGameUIPanel(false);
        StartCoroutine(gameEndUI.GameEndAnimation(isGameSuccess, cells, selectedCellIndexes));

        if (isGameSuccess)
        {
            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level") + 1);
            strikeManager.IncreaseStrike();

            PlayerPrefs.SetInt("collectedLetter",
                PlayerPrefs.GetInt("collectedLetter") + cellCount); // Harf Topla Görevi

            if (noWrongGames == 0)
            {
                PlayerPrefs.SetInt("perfectGames",
                    PlayerPrefs.GetInt("perfectGames") + 1); // Hata Yapmadan Oyun Görevi
            }
            
            if (noJokerGames == 0)
            {
                PlayerPrefs.SetInt("noJokerGames",
                    PlayerPrefs.GetInt("noJokerGames") + 1); // Joker Kullanmadan Oyun Görevi
            }

            ReturnAwardCoin();
        }
        else
        {
            PlayerPrefs.SetInt("health", PlayerPrefs.GetInt("health") - 1);

            strikeManager.ResetStrike();
        }

        userDataManager.SaveUserData();
        
    }

    public int ReturnAwardCoin(){ 
        coinManager.GameAwardCoin(cellCount, moveCount);
        return cellCount * 2 + moveCount * 5;
    }


    public void ConfigureCardCells(GameObject[] cells, int userLevel){
        LoadLevelCellData(userLevel);
        ConfigureCells(cellData, cells);
    }

    public void ConfigureCells(string[] cellData, GameObject[] cells){
        for (int i = 0; i < cellData.Length; i++)
        {
            string normalizedCell = NormalizeString(cellData[i]).ToUpper(new CultureInfo("tr-TR"));

            bool isValidLetter = alphabet
                .Select(letter => NormalizeString(letter).ToUpper(new CultureInfo("tr-TR")))
                .Contains(normalizedCell);

            if (isValidLetter)
            {
                cellCount++;
                gameCellIndexes.Add(i);

                cells[i].SetActive(true);
            }
            else
            {
                cells[i].SetActive(false);
            }
        }
    }


    public string[] GetCellData(){
        return cellData;
    }

    public bool IsGameEnd(){
        return isGameEnd;
    }

    public List<int> GetNotUsedLetters(){
        return notUsedIndexes;
    }

    public void ListUnusedAlphabetCharsWithIndex()
    {
        // 1) cellData içinde kullanılan harfleri bir HashSet içinde toplayalım
        HashSet<string> usedLetters = new HashSet<string>();

        foreach (string cell in cellData)
        {
            if (string.IsNullOrEmpty(cell)) continue;

            foreach (char c in cell)
            {
                // Karakterin normalize edilmiş, küçük harfe dönmüş hâli:
                string normalizedChar = c
                    .ToString()
                    .Trim()
                    .Normalize(NormalizationForm.FormC)
                    .ToLower(new CultureInfo("tr-TR"));

                // Alfabe içinde var mı diye kontrol
                bool isInAlphabet = alphabet.Any(a =>
                    a
                    .ToLower(new CultureInfo("tr-TR"))
                    .Normalize(NormalizationForm.FormC) == normalizedChar
                );

                if (isInAlphabet)
                {
                    usedLetters.Add(normalizedChar);
                }
            }
        }

        for (int i = 0; i < alphabet.Length; i++)
        {
            // Alfabedeki harfi normalize ederek küçük harfe çevirip kontrol
            string normalizedLetter = alphabet[i]
                .ToLower(new CultureInfo("tr-TR"))
                .Normalize(NormalizationForm.FormC);

            if (!usedLetters.Contains(normalizedLetter))
            {
                notUsedLetters.Add(alphabet[i]);
                notUsedIndexes.Add(i);
            }
        }

        // 3) Sonucu konsolda göster
        if (notUsedLetters.Count > 0)
        {
            // Harflerin kendisi
            // Debug.Log("Alfabede olup da cellData'da kullanılmayan harfler: " + string.Join(", ", notUsedLetters));

            // Alfabedeki indexleri
            // Debug.Log("Kullanılmayan harflerin alfabedeki indexleri: " + string.Join(", ", notUsedIndexes));
        }
        else
        {
            //Debug.Log("cellData içerisinde alfabenin tüm harfleri kullanılmış!");
        }
    }

    public int GetNextClosestUnselectedCellIndex()
    {
        // Filtreleme: gameCellIndexes içinde selectedCellIndexes'de olmayanları bul
        var unselectedIndexes = gameCellIndexes.Except(selectedCellIndexesFromPlayer);

        // Seçilmiş hücreleri hariç tut
        unselectedIndexes = unselectedIndexes.Except(selectedCellIndexes);

        // Eğer selectedCellIndexes'te en az 2 hücre varsa son iki seçili hücreyi kontrol ederek yönü belirle
        if (selectedCellIndexesFromPlayer.Count >= 2)
        {
            int lastIndex = selectedCellIndexesFromPlayer[^1];
            int secondLastIndex = selectedCellIndexesFromPlayer[^2];

            int difference = lastIndex - secondLastIndex;

            if (difference == 1)
            {
                // Yatay hareket (soldan sağa)
                var nextCell = unselectedIndexes.FirstOrDefault(index => index == lastIndex + 1);
                if (nextCell != 0) return nextCell;
            }
            else if (difference == 8)
            {
                // Dikey hareket (yukarıdan aşağıya)
                var nextCell = unselectedIndexes.FirstOrDefault(index => index == lastIndex + 8);
                if (nextCell != 0) return nextCell;
            }
        }

        // Eğer aynı kelimede uygun hücre yoksa genel olarak en yakın hücreyi bul
        var nextCellDefault = unselectedIndexes.OrderBy(index => Math.Abs(index - selectedCellIndex)).FirstOrDefault();

        return nextCellDefault != 0 || unselectedIndexes.Contains(0) ? nextCellDefault : -1;
    }

    public void SelectAnotherCell(){
        if (!(jokerAnimations.GetActiveJoker() == null))
        {
            if (jokerAnimations.GetActiveJoker().isActive)
            {
                selectedCellIndex = -1;
                return;
            } 
        }

        if(!gameCellIndexes.All(index => selectedCellIndexes.Contains(index))){
            selectedCellIndex = GetNextClosestUnselectedCellIndex();
            OnCellClicked(selectedCellIndex);
        }
    }

}
