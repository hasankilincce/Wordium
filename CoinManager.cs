using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public void ChangeCoinCount(int amount)
    {
        int currentCoinCount = PlayerPrefs.GetInt("coins", 0);
        currentCoinCount += amount;
        PlayerPrefs.SetInt("coins", currentCoinCount);
    }


    public void GameAwardCoin(int cellCount, int moveCount){
        int award = cellCount * 2 + moveCount * 5;

        ChangeCoinCount(award);
    }
}
