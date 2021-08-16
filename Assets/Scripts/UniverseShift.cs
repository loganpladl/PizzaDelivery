using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UniverseShift : MonoBehaviour
{
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
    Volume shiftPostProcessVolume;

    [SerializeField]
    GameObject[] timerBackgrounds;

    int numCharacters;

    bool universeShifting = false;
    bool choosingUniverse = false;

    int currentUniverseIndex = 0;

    public bool UniverseShifting { get => universeShifting; }
    public bool ChoosingUniverse { get => choosingUniverse; }

    public delegate void UniverseChosenStartTransition(int universeIndex);
    public UniverseChosenStartTransition universeChosenStartTransition;

    public delegate void UniverseChosenEndTransition(int universeIndex);
    public UniverseChosenEndTransition universeChosenEndTransition;

    public delegate void SameUniverseChosen();
    public SameUniverseChosen sameUniverseChosen;

    private void Awake()
    {
        numCharacters = FindObjectsOfType<Character>().Length;
    }

    private void Start()
    {
        // Activate the universe background corresponding to the number of universes (same as num characters)
        universeBackgrounds[numCharacters - 1].SetActive(true);

        shiftPostProcessVolume.weight = 0;
    }

    public void DisplayUniverseSelectionPrompt()
    {
        choosingUniverse = true;
        // Display proper universe selection prompt
        universePromptText.SetActive(true);
        universePrompts[numCharacters - 2].SetActive(true);
    }

    public void HideUniverseSelectionPrompt()
    {
        choosingUniverse = false;
        universePrompts[numCharacters - 2].SetActive(false);
        universePromptText.SetActive(false);
    }

    public void EnableShiftingPostProcessVolume()
    {
        shiftPostProcessVolume.weight = 1;
    }

    public void DisableShiftingPostProcessVolume()
    {
        shiftPostProcessVolume.weight = 0;
    }

    // index 0 = blue, 1 = red, 2 = green
    public void SetTimerBackground(int index)
    {
        // Reset timer background sprite
        timerBackgrounds[0].SetActive(false);
        timerBackgrounds[1].SetActive(false);
        timerBackgrounds[2].SetActive(false);

        // Activate proper timer background
        timerBackgrounds[index].SetActive(true);
    }

    // Universe picked where blue is index 0, red is index 1, and green is index 3
    private void UniversePicked(int index)
    {
        choosingUniverse = false;

        

        // Skip universe transition if the character is already active
        if (currentUniverseIndex == index)
        {
            Time.timeScale = 1;
            HideUniverseSelectionPrompt();
            Cursor.lockState = CursorLockMode.Locked;
            sameUniverseChosen();
            return;
        }

        currentUniverseIndex = index;

        

        // Reset location symbol
        blueUniverseHereTriple.SetActive(false);
        redUniverseHereTriple.SetActive(false);
        greenUniverseHereTriple.SetActive(false);


        // Activate proper location symboland set proper music
        if (index == 0)
        {
            blueUniverseHereTriple.SetActive(true);
            AudioManager.Instance.SetMusicBlueUniverse();
        }
        else if (index == 1)
        {
            redUniverseHereTriple.SetActive(true);
            AudioManager.Instance.SetMusicRedUniverse();
        }
        else if (index == 2)
        {
            greenUniverseHereTriple.SetActive(true);
            AudioManager.Instance.SetMusicGreenUniverse();
        }

        StartCoroutine(UniverseShiftTransition());
    }

    public void BlueUniversePicked()
    {
        UniversePicked(0);
    }

    public void RedUniversePicked()
    {
        UniversePicked(1);
    }

    public void GreenUniversePicked()
    {
        UniversePicked(2);
    }

    IEnumerator UniverseShiftTransition()
    {
        universeChosenStartTransition(currentUniverseIndex);
        EnableShiftingPostProcessVolume();

        universeShifting = true;
        HideUniverseSelectionPrompt();
        Cursor.lockState = CursorLockMode.Locked;

        // Ideally would wait until after to reset timescale but Cinemachine needs it to do the camera transition
        Time.timeScale = 1f;

        AudioManager.Instance.PlayUniverseShiftSound();

        yield return new WaitForSecondsRealtime(.5f);

        

        universeShifting = false;
        

        universeChosenEndTransition(currentUniverseIndex);
        DisableShiftingPostProcessVolume();
    }
}
