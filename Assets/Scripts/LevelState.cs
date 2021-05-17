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
    GameObject blueUniverseHere;

    [SerializeField]
    GameObject redUniverseHere;

    [SerializeField]
    GameObject greenUniverseHere;

    [SerializeField]
    GameObject[] universePrompts;

    [SerializeField]
    GameObject pauseMenu;

    int numCharacters;

    [SerializeField]
    float levelDuration = 10.0f;
    float levelTimer;

    [SerializeField]
    float rewindDuration = 3.0f;
    float rewindTimer;

    // How much faster is rewind compared to normal speed
    [SerializeField]
    float rewindSpeed = 3.0f;

    // Ratio of level duration to rewind duration. Should be some number greater than or equal to 1.
    float durationRatio;
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

    bool levelStarted = false;

    bool levelEnded = false;

    bool loopEnded = false;

    bool choosingUniverse = false;

    bool loopStarted = false;

    // Current universe index, starting at 0
    int currentUniverse = 0;

    Character activeCharacter;

    // Start is called before the first frame update
    void Start()
    {
        durationRatio = levelDuration / rewindDuration;


        numCharacters = characters.Length;
        levelTimer = levelDuration;
        //rewindTimer = rewindDuration;
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
        shiftPostProcessVolume.weight = 1;
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
                        Cursor.lockState = CursorLockMode.Locked;
                        Time.timeScale = 1;
                    }
                    else
                    {
                        pauseMenu.SetActive(true);
                        Cursor.lockState = CursorLockMode.None;
                        Time.timeScale = 0;
                    }
                }

                // Check for player rest
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
            else if (!choosingUniverse)
            {
                rewindTimer -= Time.deltaTime;
                //timerText.text = ((1 - rewindTimer / rewindDuration) * levelDuration).ToString("00.0");
                timerText.text = (levelDuration - rewindTimer * rewindSpeed).ToString("00.0");
                if (rewindTimer <= 0)
                {
                    StopRewind();
                    if (numCharacters > 1)
                    {
                        choosingUniverse = true;
                        // Display proper universe selection prompt
                        universePrompts[numCharacters - 1].SetActive(true);
                        Cursor.lockState = CursorLockMode.None;
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
        // Wait while start transition happens
        yield return new WaitForSecondsRealtime(1f);

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

        shiftPostProcessVolume.weight = 0;

        StartLoop();
    }

    private IEnumerator LoopEnd()
    {
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
        playIcon.GetComponent<Animation>().Play();
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
        universePrompts[numCharacters - 1].SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;

        activeCharacter.SetActiveCharacter();
        activeCharacter.gameObject.SetActive(true);

        rewindPostProcessVolume.weight = 0;
        shiftPostProcessVolume.weight = 1;

        // Ideally would wait until after to reset timescale but Cinemachine needs it to do the camera transition
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(.5f);

        StartLoop();
    }

    public void BlueUniversePicked()
    {
        choosingUniverse = false;

        activeCharacter.SetNotActiveCharacter();
        // TODO: Hardcoding index is bad. Should refactor how I handle multiple characters/universes
        activeCharacter = characters[0];
        inputManager.SetActiveCharacter(0);

        blueUniverseHere.SetActive(true);
        if (redUniverseHere != null)
        {
            redUniverseHere.SetActive(false);
        }
        if (greenUniverseHere != null)
        {
            greenUniverseHere.SetActive(false);
        }


        StartCoroutine(UniverseShift());
    }

    public void RedUniversePicked()
    {
        choosingUniverse = false;

        activeCharacter.SetNotActiveCharacter();
        activeCharacter = characters[1];
        inputManager.SetActiveCharacter(1);

        blueUniverseHere.SetActive(false);
        redUniverseHere.SetActive(true);
        if (greenUniverseHere != null)
        {
            greenUniverseHere.SetActive(false);
        }

        StartCoroutine(UniverseShift());
    }

    public void GreenUniversePicked()
    {
        choosingUniverse = false;

        activeCharacter.SetNotActiveCharacter();
        activeCharacter = characters[2];
        inputManager.SetActiveCharacter(2);

        blueUniverseHere.SetActive(false);
        redUniverseHere.SetActive(false);
        greenUniverseHere.SetActive(true);

        StartCoroutine(UniverseShift());
    }

    void StartRewind()
    {
        rewindIcon.SetActive(true);

        rewinding = true;

        int stepsToRewind = (int)(levelDuration-levelTimer * (1 / Time.fixedDeltaTime));
        float rewindDuration = (levelDuration - levelTimer) / rewindSpeed;

        foreach (Character c in characters)
        {
            //c.SetRewindDuration()
            //c.StartRewind(stepsToRewind, rewindDuration);
            c.StartRewind();
        }
        levelTimer = levelDuration;
    }

    void StopRewind()
    {
        rewindIcon.SetActive(false);
        
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
        knockPromptUI.SetActive(false);
        pizzaDeliveredUI.SetActive(true);
        foreach (Character c in characters)
        {
            c.Disable();
        }
        inputManager.Disable();

        StartCoroutine(SceneTransition());
    }

    IEnumerator SceneTransition()
    {
        sceneTransition.SetTrigger("LevelOver");
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
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Rewind early if the player presses Q or falls out of bounds
    public void EarlyRewind()
    {

    }
}
