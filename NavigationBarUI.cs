using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NavigationBar : MonoBehaviour
{
    public RectTransform homeButton, tasksButton, shopButton;
    public RectTransform buttonBG; // Kayacak arkaplan
    public Image buttonBGImage; // ButtonBG'nin arkaplan rengi
    public GameObject playText; // PlayText nesnesi
    public Button buttonBGButton; // ButtonBG'nin buton bileşeni

    public GameObject[] pages; // Sayfalar dizisi (sayfalar burada GameObject olarak saklanır)
    private int currentPageIndex = 0; // Başlangıç sayfası

    void Start()
    {
        homeButton.GetComponent<Button>().onClick.AddListener(() => AnimateButton(0, 0, 320, new Color(1.0f, 0.6f, 0.0f, 1f), true)); // #FF9900 (Home)
        tasksButton.GetComponent<Button>().onClick.AddListener(() => AnimateButton(1, 274, 220, new Color(0.0f, 0.75f, 0.39f, 1f), false)); // #00BF63 (Tasks)
        shopButton.GetComponent<Button>().onClick.AddListener(() => AnimateButton(2, -274, 220, new Color(0.0f, 0.65f, 0.93f, 1f), false)); // #00A6ED (Shop)
    }

    void AnimateButton(int pageIndex, float newX, float newWidth, Color newColor, bool enableButtonBG)
    {
        // Önceki sayfayı gizle
        pages[currentPageIndex].SetActive(false);

        // Yeni sayfayı göster
        pages[pageIndex].SetActive(true);
        currentPageIndex = pageIndex;

        // ButtonBG kaydır
        buttonBG.DOAnchorPos(new Vector2(newX, buttonBG.anchoredPosition.y), 0.5f).SetEase(Ease.OutBack);

        // Genişliği değiştir
        buttonBG.DOSizeDelta(new Vector2(newWidth, buttonBG.sizeDelta.y), 0.5f);

        // Renk değiştir
        buttonBGImage.DOColor(newColor, 0.3f);

        // Home butonuna tıklandıysa PlayText'i aç, diğer butonlara basıldıysa kapat
        playText.SetActive(enableButtonBG);

        // ButtonBG'nin buton özelliğini aktif/pasif yap
        buttonBGButton.interactable = enableButtonBG;
    }
}
