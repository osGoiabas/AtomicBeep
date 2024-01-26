using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private SceneTransitionMode sceneTransitionMode;

    public void StartGame()
    {
        FindObjectOfType<SoundManager>().PlaySFX("rads");
        SceneTransitioner.Instance.LoadScene("DebugRoom 1", sceneTransitionMode);
    }

    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Fechou o jogo.");
    }
}
