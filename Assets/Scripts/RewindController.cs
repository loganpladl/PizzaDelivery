using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class RewindController : MonoBehaviour
{
    // Total recorded time points derived from level duration and fixed timestep
    int totalSteps;

    bool rewinding = false;

    Character[] characters;
    List<RewindTarget> rewindTargets;
    

    [SerializeField]
    GameObject rewindIcon;

    [SerializeField]
    LevelTimers levelTimers;

    private void Awake()
    {
        characters = FindObjectsOfType<Character>();
        levelTimers = GetComponent<LevelTimers>();
    }

    // Start is called before the first frame update
    void Start()
    {
        

        totalSteps = (int)(levelTimers.LevelDuration * (1 / Time.fixedDeltaTime));

        //rewindTargets = new List<RewindTarget>();

        rewindTargets = new List<RewindTarget>(GameObject.FindObjectsOfType<RewindTarget>());

        /*
        foreach (Character c in characters)
        {
            rewindTargets.Add(c.gameObject.GetComponent<RewindTarget>());
        }
        */

        foreach (RewindTarget rt in rewindTargets)
        {
            rt.SetRewindParameters(totalSteps, levelTimers.LevelDuration / levelTimers.RewindSpeed);
        }
    }

    public void StartRewind()
    {
        rewindIcon.SetActive(true);

        AudioManager.Instance.PlayRewindAudio();

        rewinding = true;

        foreach (RewindTarget rt in rewindTargets)
        {
            rt.StartRewind(levelTimers.RewindTimer);
        }
        levelTimers.ResetLevelTimer();
        levelTimers.SetRewinding(true);
        levelTimers.StartTicking();
    }

    public void StopRewind()
    {
        rewindIcon.SetActive(false);

        AudioManager.Instance.StopRewindAudio();

        rewinding = false;
        foreach (RewindTarget rt in rewindTargets)
        {
            rt.StopRewind();
        }
        levelTimers.ResetRewindTimer();
        levelTimers.SetRewinding(false);
        levelTimers.StopTicking();

        Time.timeScale = 0f;
    }
}
