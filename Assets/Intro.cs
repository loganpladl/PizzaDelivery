using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    [SerializeField]
    CameraBeat MainCameraBeat;
    [SerializeField]
    CameraBeat UICameraBeat;

    [SerializeField]
    AudioSource introAudioSource;
    [SerializeField]
    AudioSource titleLoopAudioSource;

    // Time until vocals start
    [SerializeField] 
    float introDelay;
    float introTimer;
    bool introOver = false;

    bool titleLoopStarted = false;

    [SerializeField]
    Animator UIAnimator;

    // Start is called before the first frame update
    void Start()
    {
        MainCameraBeat.Enable();
        UICameraBeat.Enable();

        introAudioSource.Play();

        introTimer = introDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if (!introOver)
        {
            introTimer -= Time.deltaTime;
            if (introTimer < 0)
            {
                UIAnimator.SetTrigger("TitleDropIn");
                introOver = true;
            }
        }
        if (!introAudioSource.isPlaying && !titleLoopStarted)
        {
            titleLoopStarted = true;
            titleLoopAudioSource.Play();
        }
    }
}
