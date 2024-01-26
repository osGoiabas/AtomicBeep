using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    public CanvasGroup canvasgroup;

    private float timeToFade = 1f;

    SpriteRenderer rend;

    void Start()
    {
        canvasgroup = GetComponent<CanvasGroup>();
        FindObjectOfType<SoundManager>().PlaySFX("titleAtomicBeep");
        DontDestroyOnLoad(this);
    }
    public void ExitButton()
    {
        Application.Quit();
        Debug.Log("Fechou o jogo.");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("DebugRoom 1");
        //SceneManager.LoadSceneAsync("DebugRoom 1");
    }


    IEnumerator FadeIn() {
        FindObjectOfType<SoundManager>().PlaySFX("rads");
        for (float f = 0.05f; f < timeToFade + 0.05f; f += 0.05f) {
            canvasgroup.alpha = f/timeToFade;
            Debug.Log(f);
            yield return new WaitForSeconds(0.05f);
        }
        if (canvasgroup.alpha >= 1f - 0.05f) {
            canvasgroup.alpha = 1f;
            StartGame();
        }
    }
    public void FicarPreto() {
        StartCoroutine("FadeIn");
    }

    IEnumerator FadeOut()
    {
        for (float f = timeToFade; f >= -0.05f; f -= 0.05f)
        {
            canvasgroup.alpha = f/timeToFade;
            yield return new WaitForSeconds(0.05f);
        }
    }
    public void SairDoPreto()
    {
        StartCoroutine("FadeOut");
    }
}
