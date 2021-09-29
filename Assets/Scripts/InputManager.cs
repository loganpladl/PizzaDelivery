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

    // For testing discrepancies between recorded positions and current positions
    // The index represents fixed steps, and the value is the position BEFORE applying inputs on that step
    Dictionary<Character, List<Vector3>> recordedPositions = new Dictionary<Character, List<Vector3>>();

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

            // Uncomment to test replay divergence
            //TestReplayDivergence();
        }

        


        currentFixedStep++;
    }

    private void TestReplayDivergence()
    {
        foreach (Character c in characters)
        {
            if (c == characters[activeCharacterIndex])
            {
                // Determinism test
                recordedPositions[c].Add(c.GetComponent<Rigidbody>().position);
            }

            if (c != characters[activeCharacterIndex] && recordedPositions[c].Count > currentFixedStep)
            {
                Vector3 recordedPosition = recordedPositions[c][currentFixedStep];
                Vector3 playbackPosition = c.GetComponent<Rigidbody>().position;

                Debug.Log("Divergence Test: Fixed Step #" + currentFixedStep);

                Debug.Log("Character Index: " + System.Array.IndexOf(characters, c));
                if (recordedPosition == playbackPosition)
                {
                    Debug.Log("SUCCESS");
                }
                else
                {
                    Debug.Log("FAILURE. Recorded Position: " + recordedPosition.ToString("F7") + " Playback Position: " + playbackPosition.ToString("F7"));
                }
            }
        }
    }

    // Executes commands in fixed update after creating them in Update
    private void ExecuteActivePlayerCommands()
    {
        Character activeCharacter = characters[activeCharacterIndex];

        // Determinism test
        //recordedPositions[activeCharacter].Add(activeCharacter.GetComponent<Rigidbody>().position);

        while (commands[activeCharacter].Count > activeCharacterCommandIndex)
        {
            // Replace the key value pair with the proper fixed step value since we are now in fixed update.
            // TODO: Should do this in a much cleaner way.
            commands[activeCharacter][activeCharacterCommandIndex] = new KeyValuePair<CommandPattern.Command, int>(commands[activeCharacter][activeCharacterCommandIndex].Key, currentFixedStep);

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

            // Determinism testing
            recordedPositions.Add(c, new List<Vector3>());
        }
    }

    public void StartLoop()
    {
        Character activeCharacter = characters[activeCharacterIndex];

        commands[activeCharacter].Clear();
        // Reset position recordings for accuracy testing
        recordedPositions[activeCharacter].Clear();

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
        // TODO: These current fixed steps are not valid, because they are executed on the NEXT fixed update.
        // Maybe could just add one, but to be safe I'm replacing this key value pair with a copy with the correct fixed step before the commands are executed in fixedUpdate.
        // Should look for a cleaner way to do this.
        commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, int>(movement, currentFixedStep));
        commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, int>(look, currentFixedStep));

        if (Input.GetButtonDown("Jump"))
        {
            CommandPattern.Command jump = new CommandPattern.Jump(activeCharacter);
            commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, int>(jump, currentFixedStep));
        }

        if (Input.GetButtonDown("Interact"))
        {
            CommandPattern.Command interact = new CommandPattern.Interact(activeCharacter);
            commands[activeCharacter].Add(new KeyValuePair<CommandPattern.Command, int>(interact, currentFixedStep));
        }
    }

    void Playback()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            Character currentCharacter = characters[i];

            /*
            // Determinism testing TODO: Only want to do quick test on blue player right now but should test all of them properly
            if (currentFixedStep < recordedPositions[currentCharacter].Count)
            {
                if (i != activeCharacterIndex && currentCharacter == characters[0] && currentCharacter.GetComponent<Rigidbody>().position != recordedPositions[currentCharacter][currentFixedStep])
                {
                    Debug.Log("ERROR: Position Discrepancy at fixed step #" + currentFixedStep);
                    Debug.Log("Recorded Position: " + recordedPositions[currentCharacter][currentFixedStep].ToString("F6") + " Playback position: " + currentCharacter.GetComponent<Rigidbody>().position.ToString("F6"));
                }
            }
            */

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
                        if (currentFixedStep != nextPair.Value)
                        {
                            Debug.Log("ERROR");
                        }

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