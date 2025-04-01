using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameEndUI : MonoBehaviour
{
    public SoundManager soundManager;
    public StrikeManager strikeManager;
    public GameManager gameManager;

    public CellAnimations cellAnimations;
    public RandomColor randomColor;
    public ParticleSystemManager particleSystemManager;


    public GameObject gameEndPanel;
    public GameObject gameEndSuccessPanel;
    public GameObject gameEndFailPanel;
    public TMP_Text strikeCountText;
    public TMP_Text awardCoinText;
    public TMP_Text congratulationText;

    // Start is called before the first frame update
    public void CloseGameEndPanel(){
        gameEndPanel.SetActive(false);
    }
    
    public IEnumerator GameEndAnimation(bool isGameSuccess, GameObject[] cells, List<int> selectedCellIndexes)
    {   
        yield return new WaitForSeconds(1.0f);
        
        gameEndPanel.SetActive(true);

        GameEndPanelController(isGameSuccess);

        if(isGameSuccess){
            gameEndSuccessPanel.SetActive(true);
            gameEndFailPanel.SetActive(false);

            for (int i = 0; i < selectedCellIndexes.Count; i++)
            {
                cellAnimations.CellAnimation(cells[selectedCellIndexes[i]], randomColor.GetRandomColor(), Color.white);
                yield return new WaitForSeconds(0.05f);
            }
        }
        else{
            gameEndSuccessPanel.SetActive(false);
            gameEndFailPanel.SetActive(true);

            for (int i = 0; i < selectedCellIndexes.Count; i++)
            {
                cellAnimations.CellAnimation(cells[selectedCellIndexes[i]], Color.red, Color.white);

                yield return new WaitForSeconds(0.05f);
            }
        }

        
    }

    public void GameEndPanelController(bool isGameSuccess){
        if(isGameSuccess){
            strikeCountText.text = PlayerPrefs.GetInt("strikeCount").ToString();
            awardCoinText.text = gameManager.ReturnAwardCoin().ToString();
            congratulationText.text = (PlayerPrefs.GetInt("level") - 1).ToString() + ". SEVİYE TAMAMLANDI!";

            soundManager.PlayWinSound();

            particleSystemManager.FireWorkCaller();
        }
    }

}
