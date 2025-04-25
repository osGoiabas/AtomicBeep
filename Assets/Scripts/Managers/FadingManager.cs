using System.Collections;
using UnityEngine;

public class FadingManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;

    public void FadeIn() {
        StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, fadeDuration));
    }

    public void FadeOut() {
        StartCoroutine(FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, fadeDuration));
    }
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration){
        float elapsedTime = 0.0f;
        while (elapsedTime <= fadeDuration) {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsedTime / duration);
            yield return null;
        }
    } 

    void Update()
    {
        currentAlpha = Mathf.MoveTowards( currentAlpha, desiredAlpha, 2.0f * Time.deltaTime);
    }
}
