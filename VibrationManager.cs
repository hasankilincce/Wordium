using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VibrationManager : MonoBehaviour
{
    public HapticFeedbackController hapticController;
    
    public void SoftVibrate()
    {
        if (PlayerPrefs.GetInt("vibration") == 1)
        {
            hapticController.HapticLight();
            Debug.Log("Haptic efekti oynatıldı.");
        }
    }

    public void WrongVibrate()
    {
        if (PlayerPrefs.GetInt("vibration") == 1)
        {
            hapticController.HapticWarning();
            Debug.Log("Haptic efekti oynatıldı.");
        }
    }


}
