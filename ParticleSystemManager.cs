using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    public SoundManager soundManager;

    public ParticleSystem fireworkPrefab;
    public ParticleSystem explosionEffectPrefab; // Patlama efekti prefabı
    public ParticleSystem cellEffectPrefab;

    public void PlayFirework(Vector3 position)
    {
        Instantiate(fireworkPrefab, position, Quaternion.identity);

        soundManager.PlayExplodeSound();
    }

    public void PlayExplosion(Vector3 position){
        Instantiate(explosionEffectPrefab, position, Quaternion.identity);
    }

    public void PlayCellEffect(Vector3 position){
        Instantiate(cellEffectPrefab, position, Quaternion.identity);
    }


    public void FireWorkCaller(){
        StartCoroutine(FireWorkRoutine());
    }

    public IEnumerator FireWorkRoutine(){
        for (int i = 0; i < 5; i++)
        {
            float randomX = Random.Range(-2f, 2f);
            float randomY = Random.Range(-2f, 4f);

            Vector3 randomPos = new Vector3(randomX, randomY, 0);

            PlayFirework(randomPos);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
