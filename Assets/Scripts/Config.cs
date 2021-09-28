using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : MonoBehaviour
{
    public enum DifficultyLevel
    {
        Easy, Normal, Hard
    }

    DifficultyLevel chosenDifficulty;

    public DifficultyLevel CurrentDifficulty
    {
        get => chosenDifficulty;
    }

    public void ChooseDifficultyEasy()
    {
        chosenDifficulty = DifficultyLevel.Easy;
    }

    public void ChooseDifficultyNormal()
    {
        chosenDifficulty = DifficultyLevel.Normal;
    }

    public void ChooseDifficultyHard()
    {
        chosenDifficulty = DifficultyLevel.Hard;
    }

}
