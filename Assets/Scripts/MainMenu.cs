using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private SceneTransitionMode sceneTransitionMode;
    [SerializeField] 
    private ExitScene.PortaEmQueVaiSpawnar _portaEmQueVaiSpawnar;

    public void StartGame()
    {
        FindObjectOfType<SoundManager>().PlaySFX("rads");
        SceneTransitioner.Instance.LoadScene("DebugRoom 1", _portaEmQueVaiSpawnar, sceneTransitionMode);
    }

    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Fechou o jogo.");
    }
}
