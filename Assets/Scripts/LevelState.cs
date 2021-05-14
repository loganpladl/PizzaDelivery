using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LevelState : MonoBehaviour
{
    [SerializeField]
    Character[] characters;

    int numCharacters;

    [SerializeField]
    float levelDuration = 10.0f;
    float levelTimer;

    [SerializeField]
    float rewindDuration = 3.0f;
    float rewindTimer;

    // Ratio of level duration to rewind duration. Should be some number greater than or equal to 1.
    float durationRatio;
    // Total recorded time points derived from level duration and fixed timestep
    int totalSteps;

    bool rewinding = false;

    [SerializeField]
    TextMeshProUGUI timerText;

    [SerializeField]
    Volume rewindPostProcessVolume;

    // Start is called before the first frame update
    void Start()
    {
        durationRatio = levelDuration / rewindDuration;


        numCharacters = characters.Length;
        levelTimer = levelDuration;
        rewindTimer = rewindDuration;

        totalSteps = (int)levelDuration * (int)(1 / Time.fixedDeltaTime);

        foreach (Character c in characters)
        {
            c.SetRewindParameters(totalSteps, rewindDuration);
        }

        rewindPostProcessVolume.weight = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!rewinding)
        {
            levelTimer -= Time.deltaTime;
            timerText.text = levelTimer.ToString("00.0");
            if (levelTimer <= 0)
            {
                StartRewind();
            }
        }
        else
        {
            rewindTimer -= Time.deltaTime;
            timerText.text = ((1 - rewindTimer/rewindDuration) * levelDuration).ToString("00.0");
            if (rewindTimer <= 0)
            {
                StopRewind();
            }
        }
    }

    void StartRewind()
    {
        rewinding = true;
        foreach (Character c in characters)
        {
            c.StartRewind();
        }
        levelTimer = levelDuration;

        rewindPostProcessVolume.weight = 1;
    }

    void StopRewind()
    {
        rewinding = false;
        foreach (Character c in characters)
        {
            c.StopRewind();
        }
        rewindTimer = rewindDuration;

        rewindPostProcessVolume.weight = 0;
    }
}
