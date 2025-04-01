using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //[SerializeField]
    //private SceneTransitionMode sceneTransitionMode;
    //[SerializeField] 
    //private ExitScene.PortaEmQueVaiSpawnar _portaEmQueVaiSpawnar;

    public void StartGame()
    {
        FindFirstObjectByType<SoundManager>().PlaySFX("rads");
        GameInput.PlayerInput.SwitchCurrentActionMap("Player");
        //SceneTransitioner.Instance.LoadScene("DebugRoom 1", _portaEmQueVaiSpawnar, sceneTransitionMode);
        SceneManager.LoadScene(1); //DebugRoom 1
    }

    public void ExitButton()
    {
        Debug.Log("Fechou o jogo.");
        Application.Quit();
    }
}
