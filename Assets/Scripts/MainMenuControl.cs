using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuControl : MonoBehaviour
{
    [SerializeField] GameObject fadeOut;
    void Start()
    {

    }


    void Update()
    {

    }
    public void StartGame()
    {
        StartCoroutine(StartButton());
    }

    IEnumerator StartButton()
    {
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
    }
}
