using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        FindObjectOfType<SoundManager>().PlaySFX("titleAtomicBeep");
    }

    public void ExitButton() {
        Application.Quit();
        Debug.Log("Fechou o jogo.");
    }

    public void StartGame() {
        FindObjectOfType<SoundManager>().PlaySFX("rads");
        SceneManager.LoadScene("DebugRoom 1");
    } 
}
