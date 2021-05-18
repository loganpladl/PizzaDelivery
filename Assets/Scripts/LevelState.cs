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
    GameObject[] universeBackgrounds;

    [SerializeField]
    GameObject blueUniverseHereDouble;

    [SerializeField]
    GameObject redUniverseHereDouble;

    [SerializeField]
    GameObject blueUniverseHereTriple;

    [SerializeField]
    GameObject redUniverseHereTriple;

    [SerializeField]
    GameObject greenUniverseHereTriple;

    [SerializeField]
    GameObject[] universePrompts;
    [SerializeField]
    GameObject universePromptText;

    [SerializeField]
    GameObject pauseMenu;

    int numCharacters;

    [SerializeField]
    float levelDuration = 10.0f;
    float levelTimer;

    float rewindTimer;

    // How much faster is rewind compared to normal speed
    [SerializeField]
    float rewindSpeed = 3.0f;

    // Total recorded time points derived from level duration and fixed timestep
    int totalSteps;

    bool rewinding = false;

    [SerializeField]
    TextMeshProUGUI timerText;

    [SerializeField]
    Volume rewindPostProcessVolume;

    [SerializeField]
    Volume shiftPostProcessVolume;

    [SerializeField]
    GameObject knockPromptUI;

    [SerializeField]
    GameObject pizzaDeliveredUI;

    [SerializeField]
    GameObject pauseIcon;

    [SerializeField]
    GameObject rewindIcon;

    [SerializeField]
    GameObject playIcon;

    [SerializeField]
    Animator sceneTransition;

    [SerializeField]
    Animator deliveryStart;

    [SerializeField]
    GameObject[] timerBackgrounds;

    bool levelStarted = false;

    bool levelEnded = false;

    bool loopEnded = false;

    bool choosingUniverse = false;

    bool loopStarted = false;

    Character activeCharacter;

    bool universeShifting = false;

    [SerializeField]
    AudioSource soundEffectSource;

    [SerializeField]
    AudioClip knockSound;

    [SerializeField]
    AudioClip sceneTransitionSoundIn;

    [SerializeField]
    AudioClip sceneTransitionSoundOut;

    [SerializeField]
    AudioSource rewindAudioSource;

    [SerializeField]
    AudioClip loopPlaySound;

    [SerializeField]
    AudioClip loopPauseSound;

    [SerializeField]
    AudioSource musicAudioSource;

    [SerializeField]
    AudioClip victorySound;

    [SerializeField]
    AudioClip levelStartSound;

    [SerializeField]
    AudioClip musicClipBlueUniverse;

    [SerializeField]
    AudioClip musicClipRedUniverse;

    [SerializeField]
    AudioClip musicClipGreenUniverse;

    [SerializeField]
    AudioClip universeShiftSound;

    [SerializeField]
    float BlueUniverseMusicVolume = .5f;

    [SerializeField]
    float RedUniverseMusicVolume = .5f;

    [SerializeField]
    float GreenUniverseMusicVolume = .5f;

    // Start is called before the first frame update
    void Start()
    {
        numCharacters = characters.Length;
        levelTimer = levelDuration;
        timerText.text = levelTimer.ToString("00.0");
        rewindTimer = levelDuration / rewindSpeed;

        totalSteps = (int)(levelDuration * (1 / Time.fixedDeltaTime));

        foreach (Character c in characters)
        {
            c.SetRewindParameters(totalSteps, levelDuration / rewindSpeed);
        }

        rewindPostProcessVolume.weight = 0;

        // Activate the universe background corresponding to the number of universes (same as num characters)
        universeBackgrounds[characters.Length - 1].SetActive(true);

        inputManager.Init(characters);

        activeCharacter = characters[0];

        StartCoroutine(StartLevel());
        shiftPostProcessVolume.weight = 0;
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
                levelTimer -= Time.deltaTime;
                timerText.text = levelTimer.ToString("00.0");
                if (levelTimer <= 0)
                {
                    StartCoroutine(LoopEnd());
                }

                // TODO: Should handle these inputs in inputmanager but time is precious
                // Check for player pause
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (pauseMenu.activeInHierarchy)
                    {
                        pauseMenu.SetActive(false);
                        musicAudioSource.Play();
                        Cursor.lockState = CursorLockMode.Locked;
                        Time.timeScale = 1;
                    }
                    else
                    {
                        pauseMenu.SetActive(true);
                        musicAudioSource.Pause();
                        Cursor.lockState = CursorLockMode.None;
                        Time.timeScale = 0;
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
            else if (!choosingUniverse && !universeShifting)
            {
                rewindTimer -= Time.deltaTime;

                timerText.text = (levelDuration - rewindTimer * rewindSpeed).ToString("00.0");
                if (rewindTimer <= 0)
                {
                    StopRewind();
                    if (numCharacters > 1)
                    {
                        choosingUniverse = true;
                        // Display proper universe selection prompt
                        universePromptText.SetActive(true);
                        universePrompts[numCharacters - 2].SetActive(true);
                        Cursor.lockState = CursorLockMode.None;
                    }
                    else
                    {
                        // Skip universe selection and start new loop
                        // TODO: Encapsulate thing alongside the identical code to skip universe shifting if the player chooses the same universe
                        Time.timeScale = 1;
                        playIcon.GetComponent<Animation>().Play();
                        soundEffectSource.PlayOneShot(loopPlaySound, .5f);
                        rewindPostProcessVolume.weight = 0;
                        Cursor.lockState = CursorLockMode.Locked;
                        StartLoop();
                    }
                }
            }
            else if (choosingUniverse)
            {

            }
        }
    }

    IEnumerator StartLevel()
    {
        //soundEffectSource.PlayOneShot(sceneTransitionSoundOut, .45f);

        musicAudioSource.clip = musicClipBlueUniverse;
        musicAudioSource.volume = BlueUniverseMusicVolume;

        // Fix weird glitch where the start animation isn't playing right away maybe?
        Time.timeScale = 1;

        // Wait while start transition happens
        yield return new WaitForSecondsRealtime(.5f);

        deliveryStart.SetTrigger("Start");
        

        yield return new WaitForSecondsRealtime(.4f);
        // Level start sound is best here but total wait needs to be .5s
        soundEffectSource.PlayOneShot(levelStartSound, .5f);
        yield return new WaitForSecondsRealtime(.1f);

        levelStarted = true;
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

        //shiftPostProcessVolume.weight = 1;

        StartLoop();
    }

    private IEnumerator LoopEnd()
    {
        // Hide knock prompt in case the player was looking at the door when time ran out
        HideKnockPrompt();

        soundEffectSource.PlayOneShot(loopPauseSound, .3f);
        musicAudioSource.Stop();

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
        StartRewind();

        loopStarted = false;
    }

    public void StartLoop()
    {
        musicAudioSource.Play();

        loopEnded = false;
        
        shiftPostProcessVolume.weight = 0;

        inputManager.Enable();
        inputManager.StartLoop();

        foreach (Character c in characters)
        {
            if (c.gameObject.activeInHierarchy)
            {
                c.Enable();
            }
            
        }

        // fixes bug where rewinding doesnt start at 0 but at like 1.7secs?
        rewindTimer = levelDuration / rewindSpeed;

        loopStarted = true;

        Time.timeScale = 1f;
    }

    IEnumerator UniverseShift()
    {
        universeShifting = true;

        universePrompts[numCharacters - 2].SetActive(false);
        universePromptText.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;

        activeCharacter.SetActiveCharacter();
        activeCharacter.gameObject.SetActive(true);

        rewindPostProcessVolume.weight = 0;
        shiftPostProcessVolume.weight = 1;

        // Ideally would wait until after to reset timescale but Cinemachine needs it to do the camera transition
        Time.timeScale = 1f;

        soundEffectSource.PlayOneShot(universeShiftSound, .5f);

        yield return new WaitForSecondsRealtime(.5f);

        // Play animation here instead of StartLoop to avoid playing when the StartLevel function calls StartLoop()
        playIcon.GetComponent<Animation>().Play();
        soundEffectSource.PlayOneShot(loopPlaySound, .5f);

        universeShifting = false;
        StartLoop();
    }

    public void BlueUniversePicked()
    {
        choosingUniverse = false;

        musicAudioSource.clip = musicClipBlueUniverse;
        musicAudioSource.volume = BlueUniverseMusicVolume;

        // Skip universe transition if the character is already active
        if (activeCharacter == characters[0])
        {
            Time.timeScale = 1;
            playIcon.GetComponent<Animation>().Play();
            rewindPostProcessVolume.weight = 0;
            universePrompts[numCharacters - 2].SetActive(false);
            universePromptText.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            soundEffectSource.PlayOneShot(loopPlaySound, .5f);
            StartLoop();
            return;
        }

        activeCharacter.SetNotActiveCharacter();
        // TODO: Hardcoding index is bad. Should refactor how I handle multiple characters/universes
        activeCharacter = characters[0];
        inputManager.SetActiveCharacter(0);

        if (numCharacters == 2)
        {
            blueUniverseHereDouble.SetActive(true);
            redUniverseHereDouble.SetActive(false);

            timerBackgrounds[0].SetActive(true);
            timerBackgrounds[1].SetActive(false);
        }
        else if (numCharacters == 3)
        {
            blueUniverseHereTriple.SetActive(true);
            redUniverseHereTriple.SetActive(false);
            greenUniverseHereTriple.SetActive(false);

            timerBackgrounds[0].SetActive(true);
            timerBackgrounds[1].SetActive(false);
            timerBackgrounds[2].SetActive(false);
        }

        StartCoroutine(UniverseShift());
    }

    public void RedUniversePicked()
    {
        choosingUniverse = false;

        musicAudioSource.clip = musicClipRedUniverse;
        musicAudioSource.volume = RedUniverseMusicVolume;

        // Skip universe transition if the character is already active
        if (activeCharacter == characters[1])
        {
            Time.timeScale = 1;
            playIcon.GetComponent<Animation>().Play();
            rewindPostProcessVolume.weight = 0;
            universePrompts[numCharacters - 2].SetActive(false);
            universePromptText.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            soundEffectSource.PlayOneShot(loopPlaySound, .5f);
            StartLoop();
            return;
        }

        activeCharacter.SetNotActiveCharacter();
        activeCharacter = characters[1];
        inputManager.SetActiveCharacter(1);

        if (numCharacters == 2)
        {
            blueUniverseHereDouble.SetActive(false);
            redUniverseHereDouble.SetActive(true);

            timerBackgrounds[0].SetActive(false);
            timerBackgrounds[1].SetActive(true);
        }
        else if (numCharacters == 3)
        {
            blueUniverseHereTriple.SetActive(false);
            redUniverseHereTriple.SetActive(true);
            greenUniverseHereTriple.SetActive(false);

            timerBackgrounds[0].SetActive(false);
            timerBackgrounds[1].SetActive(true);
            timerBackgrounds[2].SetActive(false);
        }

        StartCoroutine(UniverseShift());
    }

    public void GreenUniversePicked()
    {
        choosingUniverse = false;

        musicAudioSource.clip = musicClipGreenUniverse;
        musicAudioSource.volume = GreenUniverseMusicVolume;

        // Skip universe transition if the character is already active
        if (activeCharacter == characters[2])
        {
            Time.timeScale = 1;
            playIcon.GetComponent<Animation>().Play();
            rewindPostProcessVolume.weight = 0;
            universePrompts[numCharacters - 2].SetActive(false);
            universePromptText.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            soundEffectSource.PlayOneShot(loopPlaySound, .5f);
            StartLoop();
            return;
        }

        activeCharacter.SetNotActiveCharacter();
        activeCharacter = characters[2];
        inputManager.SetActiveCharacter(2);

        blueUniverseHereTriple.SetActive(false);
        redUniverseHereTriple.SetActive(false);
        greenUniverseHereTriple.SetActive(true);

        timerBackgrounds[0].SetActive(false);
        timerBackgrounds[1].SetActive(false);
        timerBackgrounds[2].SetActive(true);

        StartCoroutine(UniverseShift());
    }

    void StartRewind()
    {
        rewindIcon.SetActive(true);

        rewindAudioSource.time = 0;
        rewindAudioSource.Play();

        rewinding = true;

        foreach (Character c in characters)
        {
            c.StartRewind(rewindTimer);
        }
        levelTimer = levelDuration;
    }

    void StopRewind()
    {
        rewindIcon.SetActive(false);

        rewindAudioSource.Stop();

        rewinding = false;
        foreach (Character c in characters)
        {
            c.StopRewind();
        }
        rewindTimer = levelDuration / rewindSpeed;

        Time.timeScale = 0f;
    }

    public void Victory()
    {
        levelEnded = true;
        HideKnockPrompt();
        pizzaDeliveredUI.SetActive(true);
        foreach (Character c in characters)
        {
            c.Disable();
            c.CancelVelocity();
        }
        inputManager.Disable();

        soundEffectSource.PlayOneShot(knockSound, .6f);
        musicAudioSource.Stop();
        soundEffectSource.PlayOneShot(victorySound, .4f);

        StartCoroutine(SceneTransition());
    }

    IEnumerator SceneTransition()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        sceneTransition.SetTrigger("StartTransition");
        // Sound effect plays a little early so delay for .1s, total wait from transition start to scene load needs to remain 1 second
        //soundEffectSource.PlayOneShot(sceneTransitionSoundIn, .45f);
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

    public bool IsLevelStarted()
    {
        return levelStarted;
    }

    public bool IsLevelOver()
    {
        return levelEnded;
    }

    public float GetTimeSinceLoopStart()
    {
        return levelDuration - levelTimer;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        musicAudioSource.Play();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Rewind early if the player presses Q or falls out of bounds
    public void EarlyRewind()
    {
        rewindTimer = (levelDuration - levelTimer) / rewindSpeed;
        StartCoroutine(LoopEnd());
    }
}