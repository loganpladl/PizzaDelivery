using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Instructions : MonoBehaviour
{
    [SerializeField]
    GameObject textOne;
    [SerializeField]
    GameObject textTwo;

    [SerializeField]
    Animator sceneTransition;

    [SerializeField]
    AudioSource clickAudioSource;

    int currentPage = 0;


    public void Continue()
    {
        if (currentPage == 0)
        {
            textOne.SetActive(false);
            textTwo.SetActive(true);
            clickAudioSource.Play();
        }
        else if (currentPage == 1)
        {
            StartCoroutine(StartGame());
            clickAudioSource.Play();
        }
        currentPage++;
    }

    private IEnumerator StartGame()
    {
        sceneTransition.SetTrigger("StartTransition");
        yield return new WaitForSecondsRealtime(1.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
