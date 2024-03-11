using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private SceneTransitionMode sceneTransitionMode;
    [SerializeField] 
    private ExitScene.PortaEmQueVaiSpawnar _portaEmQueVaiSpawnar;

    public void StartGame()
    {
        FindObjectOfType<SoundManager>().PlaySFX("rads");
        GameInput.PlayerInput.SwitchCurrentActionMap("Player");
        SceneTransitioner.Instance.LoadScene("DebugRoom 1", _portaEmQueVaiSpawnar, sceneTransitionMode);
    }

    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Fechou o jogo.");
    }
}
