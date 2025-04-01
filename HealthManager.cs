using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    private int userHealth;

    public void CheckUserHealthCount(){
        userHealth = PlayerPrefs.GetInt("health");
    }

    public bool isHealthCountEnough(){
        CheckUserHealthCount();
        
        return userHealth > 0;
    }
}
