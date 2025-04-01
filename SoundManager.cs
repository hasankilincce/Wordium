using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Tek bir AudioSource üzerinden `PlayOneShot` kullanarak aynı anda birden çok ses oynatabilirsiniz.
    [SerializeField] private AudioSource audioSource;

    // Örneğin çeşitli ses klipleri:
    [SerializeField] private AudioClip PopSound;
    [SerializeField] private AudioClip toggleSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip explodeSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip switchSound;
    // veya dizi / dictionary kullanarak daha organize da olabilirsiniz


    // Buton tıklama sesi
    public void PlayPopSound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            audioSource.PlayOneShot(PopSound);
        }
    }

    // Toggle ya da Checkbox değiştirme sesi
    public void PlayToggleSound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            audioSource.PlayOneShot(toggleSound);
        }   
    }

    // Slider sürükleme, seçme vb. için
    public void PlayWrongSound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
           audioSource.PlayOneShot(wrongSound);
        }  
    }

    public void PlayCorrectSound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            audioSource.PlayOneShot(correctSound);
        }  
    }

    public void PlayExplodeSound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            audioSource.PlayOneShot(explodeSound);
        }  
    }

    public void PlayWinSound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            audioSource.PlayOneShot(winSound);
        }   
    }

    public void PlaySwitchSound()
    {
        if (PlayerPrefs.GetInt("sound") == 1)
        {
            audioSource.PlayOneShot(switchSound);
        }   
    }

    // Projede başka sesler gerekiyorsa benzer fonksiyonlar ekleyebilirsiniz
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
