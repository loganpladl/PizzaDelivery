using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hub : MonoBehaviour
{
    //TODO: Only an array to satisfy InputManager.Input parameter
    [SerializeField] Character[] players;
    [SerializeField] InputManager input;

    // Start is called before the first frame update
    void Start()
    {
        input.Init(players);
        input.Enable();
        players[0].Enable();
        players[0].DisableRendering();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
