using UnityEngine;

public class IncrementTimer : MonoBehaviour
{
    private const string LastUpdateTimeKey = "LastUpdateTime";

    private const int IncrementIntervalMinutes = 25;
    private const int MaxValue = 5;

    private float checkTimer = 0f;
    private const float checkInterval = 60f; // her dakika kontrol eder

    void Start()
    {
        UpdateIncrementValue();
    }

    void Update()
    {
        int userHealth = PlayerPrefs.GetInt("health");

        if (userHealth < MaxValue)
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                UpdateIncrementValue();
            }
        }
    }

    void UpdateIncrementValue()
    {
        int userHealth = PlayerPrefs.GetInt("health");

        if (userHealth >= MaxValue)
            return;

        string lastTimeStr = PlayerPrefs.GetString(LastUpdateTimeKey);

        System.DateTime lastTime;

        if (string.IsNullOrEmpty(lastTimeStr))
        {
            lastTime = System.DateTime.Now;
            PlayerPrefs.SetString(LastUpdateTimeKey, lastTime.ToString());
            PlayerPrefs.Save();
            return;
        }
        else
        {
            lastTime = System.DateTime.Parse(lastTimeStr);
        }

        System.TimeSpan elapsed = System.DateTime.Now - lastTime;

        int incrementsToAdd = (int)(elapsed.TotalMinutes / IncrementIntervalMinutes);

        if (incrementsToAdd > 0)
        {
            userHealth += incrementsToAdd;
            if (userHealth > MaxValue)
                userHealth = MaxValue;

            PlayerPrefs.SetInt("health", userHealth);

            lastTime = lastTime.AddMinutes(incrementsToAdd * IncrementIntervalMinutes);
            PlayerPrefs.SetString(LastUpdateTimeKey, lastTime.ToString());

            PlayerPrefs.Save();
        }
    }

    public float GetSecondsToNextIncrement()
    {
        int userHealth = PlayerPrefs.GetInt("health");

        if (userHealth >= MaxValue)
            return 0f;

        string lastTimeStr = PlayerPrefs.GetString(LastUpdateTimeKey);

        if (string.IsNullOrEmpty(lastTimeStr))
            return IncrementIntervalMinutes * 60f;

        System.DateTime lastTime = System.DateTime.Parse(lastTimeStr);
        System.TimeSpan elapsed = System.DateTime.Now - lastTime;

        float totalIncrementSeconds = IncrementIntervalMinutes * 60f;

        float remainingSeconds = totalIncrementSeconds - (float)(elapsed.TotalSeconds % totalIncrementSeconds);

        return remainingSeconds;
    }
}