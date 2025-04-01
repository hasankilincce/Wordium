using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ProgressBarUI : MonoBehaviour
{
    [Header("References")]
    public Image fillImage; 
    public TMP_Text percentageText; 
    public TMP_Text progressText; 

    [Header("Settings")]
    public float maxWidth = 760f; // Full width of the bar
    [Range(0f, 1f)]
    public float fillAmount = 0.0f; // Starting fill amount

    /// <summary>
    /// Show how many tasks have been completed out of total.
    /// Example usage: SetProgress(2, 5) -> 2/5, 40% fill, etc.
    /// </summary>
    public void SetProgress(int tasksCompleted, int totalTasks)
    {
        // Avoid division by zero
        if (totalTasks <= 0)
        {
            fillAmount = 0f; 
        }
        else
        {
            float ratio = (float)tasksCompleted / totalTasks;
            fillAmount = Mathf.Clamp01(ratio);
        }

        // Calculate the target width for the fill image
        float targetWidth = maxWidth * fillAmount;

        // Animate the fill bar width
        fillImage.rectTransform
                 .DOSizeDelta(new Vector2(targetWidth, fillImage.rectTransform.sizeDelta.y), 0.5f)
                 .SetEase(Ease.OutQuad);

        // Update percentage text (e.g., "40%")
        percentageText.text = Mathf.RoundToInt(fillAmount * 100) + "%";

        // Update progress text (e.g., "2 / 5 Görev Tamamlandı")
        progressText.text = tasksCompleted + " / " + totalTasks + " Görev Tamamlandı";
    }
}
