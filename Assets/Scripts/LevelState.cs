using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LevelState : MonoBehaviour
{
    [SerializeField]
    Character[] characters;

    [SerializeField]
    InputManager inputManager;

    [SerializeField]
    GameObject pauseMenu;

    int numCharacters;

    bool rewinding = false;

    [SerializeField]
    GameObject knockPromptUI;
    [SerializeField]
    GameObject pickupPromptUI;
    [SerializeField]
    GameObject cantDeliverPromptUI;

    [SerializeField]
    GameObject pizzaDroppedUI;

    [SerializeField]
    GameObject pizzaDeliveredUI;

    [SerializeField]
    GameObject pauseIcon;

    [SerializeField]
    GameObject playIcon;

    [SerializeField]
    Animator sceneTransition;

    [SerializeField]
    Animator deliveryStart;

    bool levelStarted = false;

    bool levelEnded = false;

    bool loopEnded = false;
    
    bool loopStarted = false;

    Character activeCharacter;

    // Flag to start loop in fixed update to synchronize replays
    bool startLoopNextFixedUpdate = false;

    [SerializeField]
    Volume rewindPostProcessVolume;

    RewindController rewindController;
    UniverseShift universeShift;

    [SerializeField]
    LevelTimers levelTimers;

    // Reset Unity's random seed to this number every loop
    [SerializeField]
    int initialRandomSeed;

    private void Awake()
    {
        levelTimers = GetComponent<LevelTimers>();
        rewindController = GetComponent<RewindController>();
        universeShift = GetComponent<UniverseShift>();
    }

    // Start is called before the first frame update
    void Start()
    {
        numCharacters = characters.Length;

        // TODO: Put rewind post process volume handling in RewindController.cs
        rewindPostProcessVolume.weight = 0;

        universeShift.sameUniverseChosen += SameUniverseChosen;
        universeShift.universeChosenStartTransition += UniverseChosenStartTransition;
        universeShift.universeChosenEndTransition += UniverseChosenEndTransition;

        inputManager.Init(characters);

        activeCharacter = characters[0];

        StartCoroutine(StartLevel());
        
    }

    void FixedUpdate()
    {
        if (startLoopNextFixedUpdate)
        {
            // Start the level if it hasn't started already
            if (levelStarted == false)
            {
                levelStarted = true;
            }

            startLoopNextFixedUpdate = false;
            StartLoopFixed();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Wait for level to get started by coroutine called in start method
        if (!levelStarted)
        {
            return;
        }
        else if (levelStarted && !levelEnded)
        {
            if (loopStarted && !rewinding && !loopEnded)
            {
                if (levelTimers.IsLevelTimerOver())
                {
                    StartCoroutine(LoopEnd());
                }

                // TODO: Should handle these inputs in inputmanager but time is precious
                // Check for player pause
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (pauseMenu.activeInHierarchy)
                    {
                        Unpause();
                    }
                    else
                    {
                        Pause();
                    }
                }

                // Check for player reset
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }

                // Check for player rewind
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    EarlyRewind();
                }
            }
            else if (!universeShift.ChoosingUniverse && !universeShift.UniverseShifting)
            {
                if (levelTimers.IsRewindTimerOver())
                {
                    rewindController.StopRewind();
                    if (numCharacters > 1)
                    {
                        universeShift.DisplayUniverseSelectionPrompt();
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        // Skip universe selection and start new loop
                        // TODO: Encapsulate thing alongside the identical code to skip universe shifting if the player chooses the same universe
                        Time.timeScale = 1;
                        playIcon.GetComponent<Animation>().Play();
                        AudioManager.Instance.PlayLoopPlaySound();
                        rewindPostProcessVolume.weight = 0;
                        Cursor.lockState = CursorLockMode.Locked;
                        StartLoop();
                    }
                }
            }
        }
    }

    void Pause()
    {
        pauseMenu.SetActive(true);
        AudioManager.Instance.PauseMusic();
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

    public void Unpause()
    {
        pauseMenu.SetActive(false);
        AudioManager.Instance.PlayMusic();
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }

    IEnumerator StartLevel()
    {
        AudioManager.Instance.SetMusicBlueUniverse();

        // Fix weird glitch where the start animation isn't playing right away maybe?
        Time.timeScale = 1;

        // Wait while start transition happens
        yield return new WaitForSecondsRealtime(.5f);

        deliveryStart.SetTrigger("Start");

        yield return new WaitForSecondsRealtime(.4f);
        // Level start sound is best here but total wait needs to be .5s
        AudioManager.Instance.PlayLevelStartSound();
        yield return new WaitForSecondsRealtime(.1f);

        foreach (Character c in characters)
        {
            if (c == activeCharacter)
            {
                c.SetActiveCharacter();
            }
            else
            {
                c.SetNotActiveCharacter();
                c.gameObject.SetActive(false);
            }
        }

        StartLoop();
    }

    private IEnumerator LoopEnd()
    {
        // Hide knock prompt in case the player was looking at the door when time ran out
        HideKnockPrompt();
        levelTimers.StopTicking();

        AudioManager.Instance.PlayLoopPauseSound();
        AudioManager.Instance.StopMusic();

        inputManager.Disable();

        foreach (Character c in characters)
        {
            c.Disable();
        }

        pauseIcon.SetActive(true);

        rewindPostProcessVolume.weight = 1;

        loopEnded = true;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;

        pauseIcon.SetActive(false);

        levelTimers.UpdateRewindFrac();
        levelTimers.SetEarlyRewindTimer();

        rewindController.StartRewind();

        loopStarted = false;
    }

    public void StartLoop()
    {
        startLoopNextFixedUpdate = true;
    }

    // Start loop synchronized to fixed Update to guarantee determinism
    public void StartLoopFixed()
    {
        AudioManager.Instance.PlayMusic();

        loopEnded = false;

        foreach (Character c in characters)
        {
            if (c.gameObject.activeInHierarchy)
            {
                c.Enable();
            }

        }

        inputManager.Enable();
        inputManager.StartLoop();

        // fixes bug where rewinding doesnt start at 0 but at like 1.7secs?
        levelTimers.ResetRewindTimer();

        loopStarted = true;
        levelTimers.StartTicking();

        Random.InitState(initialRandomSeed);

        Time.timeScale = 1f;
    }

    void ShowPlayIconAndSound()
    {
        playIcon.GetComponent<Animation>().Play();
        AudioManager.Instance.PlayLoopPlaySound();
    }

    public void Victory()
    {
        levelTimers.StopTicking();

        levelEnded = true;
        HideKnockPrompt();
        pizzaDeliveredUI.SetActive(true);
        foreach (Character c in characters)
        {
            c.Disable();
            c.CancelVelocity();
        }
        inputManager.Disable();

        AudioManager.Instance.PlayKnockSound();
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayVictorySound();

        StartCoroutine(SceneTransition());
    }

    IEnumerator SceneTransition()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        sceneTransition.SetTrigger("StartTransition");
        yield return new WaitForSecondsRealtime(1.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void DisplayKnockPrompt()
    {
        knockPromptUI.SetActive(true);
    }

    public void HideKnockPrompt()
    {
        knockPromptUI.SetActive(false);
    }

    public void DisplayPickupPrompt()
    {
        pickupPromptUI.SetActive(true);
    }

    public void HidePickupPrompt()
    {
        pickupPromptUI.SetActive(false);
    }

    public bool IsLevelStarted()
    {
        return levelStarted;
    }

    public bool IsLevelOver()
    {
        return levelEnded;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Rewind early if the player presses Q or falls out of bounds
    public void EarlyRewind()
    {
        //levelTimers.SetEarlyRewindTimer();
        StartCoroutine(LoopEnd());
    }

    void SameUniverseChosen()
    {
        rewindPostProcessVolume.weight = 0;
        ShowPlayIconAndSound();
        StartLoop();
    }

    void UniverseChosenStartTransition(int index)
    {
        rewindPostProcessVolume.weight = 0;

        activeCharacter.SetNotActiveCharacter();
        // TODO: Hardcoding index is bad. Should refactor how I handle multiple characters/universes
        activeCharacter = characters[index];
        inputManager.SetActiveCharacter(index);

        activeCharacter.SetActiveCharacter();
        activeCharacter.gameObject.SetActive(true);
    }

    void UniverseChosenEndTransition(int index)
    {
        // Play animation here instead of StartLoop to avoid playing when the StartLevel function calls StartLoop()
        ShowPlayIconAndSound();

        StartLoop();
    }

    public void PizzaDropped()
    {
        StartCoroutine(ShowPizzaDroppedUI());
    }

    IEnumerator ShowPizzaDroppedUI()
    {
        pizzaDroppedUI.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        pizzaDroppedUI.SetActive(false);
    }

    public void DisplayCantDeliverPrompt()
    {
        cantDeliverPromptUI.SetActive(true);
    }

    public void HideCantDeliverPrompt()
    {
        cantDeliverPromptUI.SetActive(false);
    }
}
