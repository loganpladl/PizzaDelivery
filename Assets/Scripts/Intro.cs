using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SerializeField]
    AudioSource clickAudioSource;

    // Time until vocals start
    [SerializeField] 
    float introDelay;
    float introTimer;
    bool introOver = false;

    bool titleLoopStarted = false;

    [SerializeField]
    Animator UIAnimator;

    [SerializeField]
    GameObject circleTransition;

    bool gameStarted = false;

    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject difficultyPanel;

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

    public void StartClicked()
    {
        mainPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    public void DifficultyEasyClicked()
    {
        if (!gameStarted)
        {
            Config config = GameObject.FindGameObjectWithTag("Config").GetComponent<Config>();
            config.ChooseDifficultyEasy();
            StartGame();
        }
        
    }

    public void DifficultyNormalClicked()
    {
        if (!gameStarted)
        {
            Config config = GameObject.FindGameObjectWithTag("Config").GetComponent<Config>();
            config.ChooseDifficultyNormal();
            StartGame();
        }
    }

    public void DifficultyHardClicked()
    {
        if (!gameStarted)
        {
            Config config = GameObject.FindGameObjectWithTag("Config").GetComponent<Config>();
            config.ChooseDifficultyHard();
            StartGame();
        }
    }

    void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            clickAudioSource.Play();
            StartCoroutine(StartGameCoroutine());
        }
    }

    // Load scene after transition
    private IEnumerator StartGameCoroutine()
    {
        circleTransition.GetComponent<Image>().enabled = true;
        circleTransition.GetComponent<Animator>().SetTrigger("StartTransition");
        yield return new WaitForSecondsRealtime(1.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
