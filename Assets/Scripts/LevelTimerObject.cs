using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/New Level Timer Object")]
public class LevelTimerObject : ScriptableObject
{
    [SerializeField] float timeLimitEasy;
    [SerializeField] float timeLimitNormal;
    [SerializeField] float timeLimitHard;

    float TimeLimitEasy
    {
        get => timeLimitEasy;
    }

    float TimeLimitNormal
    {
        get => timeLimitNormal;
    }

    float TimeLimitHard
    {
        get => timeLimitHard;
    }

    public float GetTimeLimit(Config.DifficultyLevel difficultyLevel)
    {
        switch(difficultyLevel)
        {
            case Config.DifficultyLevel.Easy:
                return TimeLimitEasy;
            case Config.DifficultyLevel.Normal:
                return TimeLimitNormal;
            case Config.DifficultyLevel.Hard:
                return TimeLimitHard;
            default:
                return timeLimitNormal;
        }
    }
}
