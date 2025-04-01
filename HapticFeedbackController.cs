using System.Runtime.InteropServices;
using UnityEngine;

public class HapticFeedbackController : MonoBehaviour
{
    #if UNITY_IOS && !UNITY_EDITOR
    // Her bir native fonksiyonu DllImport ile tanımlıyoruz.
    [DllImport("__Internal")]
    private static extern void TriggerHapticLight();

    [DllImport("__Internal")]
    private static extern void TriggerHapticMedium();

    [DllImport("__Internal")]
    private static extern void TriggerHapticHeavy();

    [DllImport("__Internal")]
    private static extern void TriggerHapticSuccess();

    [DllImport("__Internal")]
    private static extern void TriggerHapticWarning();

    [DllImport("__Internal")]
    private static extern void TriggerHapticError();

    [DllImport("__Internal")]
    private static extern void TriggerHapticSelection();
    #endif

    // Public metodlar
    public void HapticLight()
    {
        #if UNITY_IOS && !UNITY_EDITOR
            TriggerHapticLight();
        #endif
    }

    public void HapticMedium()
    {
        #if UNITY_IOS && !UNITY_EDITOR
            TriggerHapticMedium();
        #endif
    }

    public void HapticHeavy()
    {
        #if UNITY_IOS && !UNITY_EDITOR
            TriggerHapticHeavy();
        #endif
    }

    public void HapticSuccess()
    {
        #if UNITY_IOS && !UNITY_EDITOR
            TriggerHapticSuccess();
        #endif
    }

    public void HapticWarning()
    {
        #if UNITY_IOS && !UNITY_EDITOR
            TriggerHapticWarning();
        #endif
    }

    public void HapticError()
    {
        #if UNITY_IOS && !UNITY_EDITOR
            TriggerHapticError();
        #endif
    }

    public void HapticSelection()
    {
        #if UNITY_IOS && !UNITY_EDITOR
            TriggerHapticSelection();
        #endif
    }
}
