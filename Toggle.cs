using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening; // DOTween kullanımı öneririm

public class Toggle : MonoBehaviour, IPointerClickHandler
{
    public SoundManager soundManager;
    public VibrationManager vibrationManager;

    public Image background;
    public RectTransform handle;
    public bool isOn = false;

    [Space]
    public Color offColor = new Color32(221, 221, 221, 255);
    public Color onColor = new Color32(76, 217, 100, 255);

    public UnityEvent<bool> OnToggle;

    void Start()
    {
        UpdateToggleVisual(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSwitch();
    }

    void ToggleSwitch()
    {
        isOn = !isOn;
        UpdateToggleVisual(true);
        OnToggle?.Invoke(isOn);

        soundManager.PlaySwitchSound();
        vibrationManager.SoftVibrate();
    }

    void UpdateToggleVisual(bool animate)
    {
        background.DOColor(isOn ? onColor : offColor, animate ? 0.25f : 0f);

        float handleX = isOn ? 50f : -50f; // sağa veya sola kaydırır
        if (animate)
            handle.DOAnchorPosX(handleX, 0.25f).SetEase(Ease.InOutBack);
        else
            handle.anchoredPosition = new Vector2(handleX, 0f);
    }

    public void UpdateVisualImmediately()
    {
        UpdateToggleVisual(false); // animasyon olmadan hemen günceller
    }

}
