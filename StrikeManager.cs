using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikeManager : MonoBehaviour
{
    public JokerAnimations jokerAnimations;

    public void IncreaseStrike(){
        PlayerPrefs.SetInt("strikeCount", PlayerPrefs.GetInt("strikeCount") + 1);
    }

    public void ResetStrike()
    {
        PlayerPrefs.SetInt("strikeCount", 0);
    }


    public void ApplayStrikeRoutine(){
        int moveCount;
        if (PlayerPrefs.GetInt("strikeCount") >= 3)
        {
            moveCount = 3;
        }
        else
        {
            moveCount = PlayerPrefs.GetInt("strikeCount") + 1;
        }

        StartCoroutine(OpenRandomCells(moveCount));
    }


    IEnumerator OpenRandomCells(int moveCount){
        for (int i = 0; i < moveCount; i++)
        {
            yield return new WaitForSeconds(0.3f);
            jokerAnimations.DiceJokerClicked();
        }
    }

}
