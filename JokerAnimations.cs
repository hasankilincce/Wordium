using UnityEngine;
using DG.Tweening;

public class JokerAnimations : MonoBehaviour
{
    public GameManager gameManager;
    public ParticleSystemManager particleSystemManager;
    public JokerManager jokerManager;
    public GameUIManager gameUIManager;


    [System.Serializable]
    public class Joker
    {
        public RectTransform jokerTransform;
        public GameObject jokerBG;
        public float startY;
        public bool isActive = false;
    }

    public Joker pencilJoker;
    public Joker diceJoker;
    public Joker dictionaryJoker;
    public Joker brushJoker;

    private Joker activeJoker = null;


    void Start()
    {
        // Başlangıç pozisyonlarını kaydet
        pencilJoker.startY = pencilJoker.jokerTransform.anchoredPosition.y;
        diceJoker.startY = diceJoker.jokerTransform.anchoredPosition.y;
        dictionaryJoker.startY = dictionaryJoker.jokerTransform.anchoredPosition.y;
        brushJoker.startY = brushJoker.jokerTransform.anchoredPosition.y;

        activeJoker = pencilJoker;
    }

    public void PencilJokerClicked()
    {
        if (gameManager != null) jokerManager.OnPencilJokerClicked();
        ToggleJoker(pencilJoker);
    }

    public void DiceJokerClicked()
    {
        if (gameManager != null) jokerManager.OnDiceJokerClicked();
        ToggleJoker(diceJoker);
    }

    public void DictionaryJokerClicked()
    {
        if (gameManager != null) jokerManager.OnDictionaryJokerClicked();
        ToggleJoker(dictionaryJoker);
    }

    public void BrushJokerClicked()
    {
        if (gameManager != null) jokerManager.OnBrushJokerClicked();
        ToggleJoker(brushJoker);
    }

    // 🔄 Joker aç/kapat mantığı
    private void ToggleJoker(Joker joker)
    {
        if (joker.isActive)
        {
            StopJokerAnimation(joker);
            activeJoker = null;
        }
        else
        {
            // Önceki aktif olanı kapat
            if (activeJoker != null) StopJokerAnimation(activeJoker);
            StartJokerAnimation(joker);
            activeJoker = joker;
        }
    }

    // 🟢 Joker animasyonunu başlat
    private void StartJokerAnimation(Joker joker)
    {
        joker.isActive = true;

        if (joker == diceJoker || joker == brushJoker)
        {
            // DiceJoker için sadece 1 defa zıplama animasyonu
            joker.jokerTransform.DOAnchorPosY(joker.startY + 30f, 0.5f)
                .SetLoops(2, LoopType.Yoyo) // 1 kere yukarı çıkıp aşağı inecek (2 loop)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => StopJokerAnimation(joker));
        }
        else
        {
            gameUIManager.ShowUseJokerPanel();

            // Diğer jokerler için sonsuz zıplama
            joker.jokerTransform.DOAnchorPosY(joker.startY + 30f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);
        }

        // Büyüme animasyonu
        joker.jokerBG.transform.DOScale(Vector3.one * 1.1f, 0.5f)
            .SetEase(Ease.OutBounce);
    }

    // ⏹ Joker animasyonunu durdur ve eski haline döndür
    public void StopJokerAnimation(Joker joker)
    {
        gameUIManager.HideUseJokerPanel();

        joker.isActive = false;
        joker.jokerTransform.DOKill(true);

        // Doğrudan başlangıç pozisyonunu atayın
        joker.jokerTransform.anchoredPosition = new Vector2(joker.jokerTransform.anchoredPosition.x, joker.startY);

        // Büyüme animasyonunu tersine çevir
        joker.jokerBG.transform.DOScale(Vector3.one, 0.4f)
            .SetEase(Ease.OutBounce);
    }


    public Joker GetActiveJoker(){
        return activeJoker;
    }

}
