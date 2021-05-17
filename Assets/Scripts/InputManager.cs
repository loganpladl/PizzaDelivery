using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    float mouseSensitivity;

    [SerializeField]
    float mouseAcceleration;

    LevelState levelState;

    int numUniverses = 1;

    // index into characters of active character
    int activeCharacterIndex = 0;
    Character[] characters;

    float replayTimer = 0;
    bool enable = false;

    // Each character is associated with a list of command/time pairs
    Dictionary<Character, List<KeyValuePair<CommandPattern.Command, float>>> commands = new Dictionary<Character, List<KeyValuePair<CommandPattern.Command, float>>>();
    // Current command index for each list in the above dictionary. Reset to 0 at the beginning of each loop.
    int[] currentReplayIndices;

    // Indicates whether the currentReplayIndices have reached the end of their respective lists.
    bool[] reachedEnd;

    //private Command Horizontal, Vertical, Right, Forward, Back, Interact, Jump, Look, QuickRewind, Reset;
    // Start is called before the first frame update
    void Start()
    {
        levelState = GameObject.FindGameObjectsWithTag("LevelState")[0].GetComponent<LevelState>();
        /*
        //Bind keys with commands
        Forward = new MoveForward();
        Back = new MoveBack();
        Left = new MoveLeft();
        Right = new MoveRight();
        Look = new Look();
        //Interact = new InteractCommand();
        //buttonR = new ReplayCommand();
        */
    }

    // Update is called once per frame
    void Update()
    {
        // Hacky way to avoid doing anything when the game is paused
        if (Time.timeScale == 0)
        {
            return;
        }

        if (enable)
        {
            Record();

            Playback();
        }

    }

    public void Init(Character[] characters)
    {
        this.characters = characters;
        numUniverses = characters.Length;
        currentReplayIndices = new int[characters.Length];
        reachedEnd = new bool[characters.Length];

        foreach (Character c in characters)
        {
            commands.Add(c, new List<KeyValuePair<CommandPattern.Command, float>>());
        }
    }

    public void StartLoop()
    {
        Character activeCharacter = characters[activeCharacterIndex];

        commands[activeCharacter].Clear();

        for (int i = 0; i < currentReplayIndices.Length; i++)
        {
            currentReplayIndices[i] = 0;
            reachedEnd[i] = false;
        }
    }

    void Record()
    {
        Character activeCharacter = characters[activeCharacterIndex];
        // Get movement and look commands every update
        CommandPattern.Command movement = new CommandPattern.Movement(activeCharacter, Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float prevMouseXVelocity, prevMouseYVelocity;
        activeCharacter.GetPrevMouseVelocities(out prevMouseXVelocity, out prevMouseYVelocity);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseXVelocity = Mathf.MoveTowards(prevMouseXVelocity, mouseX, mouseAcceleration * Time.deltaTime);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        float mouseYVelocity = Mathf.MoveTowards(prevMouseYVelocity, mouseY, mouseAcceleration * Time.deltaTime);

        CommandPattern.Command look = new CommandPattern.Look(activeCharacter, mouseXVelocity, mouseYVelocity);

        float time = levelState.GetTimeSinceLoopStart();

        // Store commands alongside current time
        commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, float>(movement, time));
        commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, float>(look, time));

        movement.Execute();
        look.Execute();

        if (Input.GetButtonDown("Jump"))
        {
            CommandPattern.Command jump = new CommandPattern.Jump(activeCharacter);
            commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, float>(jump, time));
            jump.Execute();
        }
    }

    void Playback()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            Character currentCharacter = characters[i];
            // Execute recorded actions for characters other than the active one
            if (i != activeCharacterIndex)
            {
                bool executing = true;
                // Execute all commands for this character that are ready
                while (executing)
                {
                    // Cancel if no executed commands for character
                    if (commands[currentCharacter].Count == 0)
                    {
                        break;
                    }

                    int index = currentReplayIndices[i];
                    KeyValuePair<CommandPattern.Command, float> nextPair = commands[currentCharacter][index];

                    // If this command hasn't happened yet, don't execute
                    if (levelState.GetTimeSinceLoopStart() <= nextPair.Value)
                    {
                        break;
                    }

                    if (!reachedEnd[i])
                    {
                        nextPair.Key.Execute();

                        // If we've reached the end of this command list
                        if (currentReplayIndices[i] == commands[currentCharacter].Count - 1)
                        {
                            currentCharacter.StopMovementAndAnimations();
                            reachedEnd[i] = true;
                            break;
                        }
                        else
                        {
                            currentReplayIndices[i]++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    public void Enable()
    {
        enable = true;
    }

    public void Disable()
    {
        enable = false;
    }

    public void SetActiveCharacter(int index)
    {
        activeCharacterIndex = index;
    }
}