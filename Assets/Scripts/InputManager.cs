using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    float mouseSensitivity;

    [SerializeField]
    float mouseAcceleration;


    // index into characters of active character
    int activeCharacterIndex = 0;
    Character[] characters;

    bool enable = false;

    // Each character is associated with a list of command/time pairs
    Dictionary<Character, List<KeyValuePair<CommandPattern.Command, int>>> commands = new Dictionary<Character, List<KeyValuePair<CommandPattern.Command, int>>>();
    // Current command index for each list in the above dictionary. Reset to 0 at the beginning of each loop.
    int[] currentReplayIndices;

    // Indicates whether the currentReplayIndices have reached the end of their respective lists.
    bool[] reachedEnd;

    int currentFixedStep = 0;
    int activeCharacterCommandIndex = 0;

    // Start is called before the first frame update
    void Start()
    {

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
        }
        
        

    }

    // TODO: Should maybe just do everything in here for determinism? But would it make input unresponsive? Should investigate.
    private void FixedUpdate()
    {
        if (enable)
        {
            ExecuteActivePlayerCommands();

            Playback();
        }
        currentFixedStep++;
    }

    // Executes commands in fixed update after creating them in Update
    private void ExecuteActivePlayerCommands()
    {
        Character activeCharacter = characters[activeCharacterIndex];

        while (commands[activeCharacter].Count > activeCharacterCommandIndex)
        {
            CommandPattern.Command nextCommand = commands[activeCharacter][activeCharacterCommandIndex].Key;
            nextCommand.Execute();
            activeCharacterCommandIndex++;
        }
    }

    public void Init(Character[] characters)
    {
        this.characters = characters;
        currentReplayIndices = new int[characters.Length];
        reachedEnd = new bool[characters.Length];

        foreach (Character c in characters)
        {
            commands.Add(c, new List<KeyValuePair<CommandPattern.Command, int>>());
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

        currentFixedStep = 0;
        activeCharacterCommandIndex = 0;
    }

    void Record()
    {
        Character activeCharacter = characters[activeCharacterIndex];
        // Get movement and look commands every update
        CommandPattern.Command movement = new CommandPattern.Movement(activeCharacter, Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseXVelocity = mouseX;

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        float mouseYVelocity = mouseY;

        CommandPattern.Command look = new CommandPattern.Look(activeCharacter, mouseXVelocity, mouseYVelocity);

        // Store commands alongside current time
        commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, int>(movement, currentFixedStep));
        commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, int>(look, currentFixedStep));

        if (Input.GetButtonDown("Jump"))
        {
            CommandPattern.Command jump = new CommandPattern.Jump(activeCharacter);
            commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, int>(jump, currentFixedStep));
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
                    KeyValuePair<CommandPattern.Command, int> nextPair = commands[currentCharacter][index];

                    // If this command hasn't happened yet, don't execute
                    if (currentFixedStep < nextPair.Value)
                    {
                        break;
                    }
                    else if (!reachedEnd[i])
                    {
                        nextPair.Key.Execute();

                        // If we've reached the end of this command list
                        if (currentReplayIndices[i] >= commands[currentCharacter].Count - 1)
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