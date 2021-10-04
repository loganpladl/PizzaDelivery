using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelTimers : MonoBehaviour
{
    float levelDuration;

    [SerializeField]
    LevelTimerObject levelTimerObject;

    public float LevelDuration { get => levelDuration; }
    public float LevelTimer { get; private set; }
    public float RewindTimer { get; private set; }

    // How much faster is rewind compared to normal speed
    [SerializeField]
    float rewindSpeed = 3.0f;

    public float RewindSpeed { get => rewindSpeed; }

    [SerializeField]
    TextMeshProUGUI timerText;

    bool rewinding = false;
    bool ticking = false;

    void Awake()
    {
        GameObject configObject = GameObject.FindGameObjectWithTag("Config");

        if (levelTimerObject == null)
        {
            // Default 10 second duration if level timer object is not set
            levelDuration = 10.0f;
        }
        else if (configObject == null)
        {
            // Default to the normal difficulty timer if we cannot find config object
            levelDuration = levelTimerObject.GetTimeLimit(Config.DifficultyLevel.Normal);
        }
        else
        {
            Config.DifficultyLevel difficultyLevel = configObject.GetComponent<Config>().CurrentDifficulty;
            levelDuration = levelTimerObject.GetTimeLimit(difficultyLevel);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetLevelTimer();
        SetDefaultTimerText();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ticking && !rewinding)
        {
            LevelTimer -= Time.deltaTime;
            UpdateTimerText(false);
        }
        else if (ticking && rewinding)
        {
            RewindTimer -= Time.deltaTime;
            UpdateTimerText(true);
        }
        else
        {
            //SetDefaultTimerText();
        }
    }

    public void ResetLevelTimer()
    {
        LevelTimer = levelDuration;
    }

    void UpdateTimerText(bool rewinding)
    {
        if (!rewinding)
        {
            timerText.text = LevelTimer.ToString("00.0");
        }
        else
        {
            timerText.text = (levelDuration - RewindTimer * rewindSpeed).ToString("00.0");
        }
    }

    void SetDefaultTimerText()
    {
        timerText.text = levelDuration.ToString("00.0");
    }

    public void StartTicking()
    {
        ticking = true;
    }

    public void StopTicking()
    {
        ticking = false;
    }

    public bool IsLevelTimerOver()
    {
        return LevelTimer <= 0;
    }

    public bool IsRewindTimerOver()
    {
        return RewindTimer <= 0;
    }

    public void ResetRewindTimer()
    {
        RewindTimer = levelDuration / rewindSpeed;
    }

    public float GetTimeSinceLoopStart()
    {
        return levelDuration - LevelTimer;
    }

    public void SetEarlyRewindTimer()
    {
        RewindTimer = (levelDuration - LevelTimer) / rewindSpeed;
    }

    public void SetRewinding(bool rewinding)
    {
        this.rewinding = rewinding;
    }
}
