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

    int currentPage = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Continue()
    {
        if (currentPage == 0)
        {
            textOne.SetActive(false);
            textTwo.SetActive(true);
        }
        else if (currentPage == 1)
        {
            StartCoroutine(StartGame());
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
