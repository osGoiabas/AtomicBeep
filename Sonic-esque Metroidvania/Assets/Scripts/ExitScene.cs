using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScene : MonoBehaviour
{
    public string sceneToLoad;
    public string exitName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerPrefs.SetString("LastExitName", exitName);
        SceneManager.LoadScene(sceneToLoad);
    }
}
