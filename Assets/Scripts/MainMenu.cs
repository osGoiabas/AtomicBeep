using UnityEngine;

public class MainMenu : MonoBehaviour
{
    //[SerializeField]
    //private SceneTransitionMode sceneTransitionMode;
    //[SerializeField] 
    //private ExitScene.PortaEmQueVaiSpawnar _portaEmQueVaiSpawnar;

    public void StartGame()
    {
        FindObjectOfType<SoundManager>().PlaySFX("rads");
        GameInput.PlayerInput.SwitchCurrentActionMap("Player");
        //SceneTransitioner.Instance.LoadScene("DebugRoom 1", _portaEmQueVaiSpawnar, sceneTransitionMode);
    }

    public void ExitButton()
    {
        Debug.Log("Fechou o jogo.");
        Application.Quit();
    }
}
