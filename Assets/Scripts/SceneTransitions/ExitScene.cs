using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScene : MonoBehaviour
{
    public string sceneToLoad;
    public string exitName;

    [SerializeField]
    private SceneTransitionMode sceneTransitionMode;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerPrefs.SetString("LastExitName", exitName);
        SceneTransitioner.Instance.LoadScene(sceneToLoad, sceneTransitionMode);
        //SceneManager.LoadScene(sceneToLoad);
    }
}
