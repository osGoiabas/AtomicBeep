using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //[SerializeField] private SceneTransitionMode sceneTransitionMode;
    //[SerializeField] private ExitScene.PortaEmQueVaiSpawnar _portaEmQueVaiSpawnar;

    private IDataService DataService = new JsonDataService();

    public void StartGame()
    {
        //#TODO: popup perguntando "tem certeza? isso vai apagar todos os dados salvos"
        //#TODO: save slots"

        FindFirstObjectByType<SoundManager>().PlaySFX("rads");
        GameInput.PlayerInput.SwitchCurrentActionMap("Player");
        //SceneTransitioner.Instance.LoadScene("DebugRoom 1", _portaEmQueVaiSpawnar, sceneTransitionMode);
        SceneManager.LoadScene(1); //DebugRoom 1
    }

    public void LoadGame()
    {
        //load saved game data from player-stats.json
        var data = DataService.LoadData<int>("/player-stats.json", false);
        ItemManager.radsCollected = data;
        Debug.Log(data);

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
