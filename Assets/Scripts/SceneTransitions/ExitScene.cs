using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScene : MonoBehaviour
{

    [SerializeField]
    private SceneTransitionMode sceneTransitionMode;

    [SerializeField] public PortaEmQueVaiSpawnar _portaAtual;

    [Space(10f)]
    [Header("Vai para a cena:")]
    [SerializeField] private PortaEmQueVaiSpawnar _portaEmQueVaiSpawnar;
    [SerializeField] private SceneField _sceneToLoad;


    public enum PortaEmQueVaiSpawnar { 
        Nenhuma,
        Porta01,
        Porta02,
        Porta03,
        Porta04,
        Porta05,
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SceneTransitioner._loadFromDoor = true;
        SceneTransitioner.Instance.LoadScene(_sceneToLoad.SceneName, _portaEmQueVaiSpawnar, sceneTransitionMode);
    }

}
