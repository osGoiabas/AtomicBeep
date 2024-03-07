using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Objects")]
    [SerializeField] private GameObject _pauseMenuCanvas;
    [SerializeField] private GameObject _settingsMenuCanvas;

    [Header("First Selected Options")]
    [SerializeField] private GameObject _pauseMenuFirst;
    [SerializeField] private GameObject _settingsMenuFirst;


    private bool isPaused;

    private void Start()
    {
        _pauseMenuCanvas.SetActive(false);
        _settingsMenuCanvas.SetActive(false);
    }

    private void Update(){
        if (GameInput.WasPausePressed) {
            if (!isPaused) {
                Pause();
            } else {
                Unpause();
            }
        }
    }

    public void Pause() {
        isPaused = true;
        Time.timeScale = 0f;

        OpenPauseMenu();
    }

    public void Unpause(){
        isPaused = false;
        Time.timeScale = 1f;

        CloseAllMenus();
    }

    #region Open/Close Menus
    private void OpenPauseMenu(){
        _pauseMenuCanvas.SetActive(true);
        _settingsMenuCanvas.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_pauseMenuFirst);
    }

    private void OpenSettingsMenu(){
        _pauseMenuCanvas.SetActive(false);
        _settingsMenuCanvas.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_settingsMenuFirst);
    }

    private void CloseAllMenus(){
        _pauseMenuCanvas.SetActive(false);
        _settingsMenuCanvas.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }
    # endregion

    #region On Button Press
    public void OnSettingsPress(){
        OpenSettingsMenu();
    }

    public void OnSettingsBackPress(){
        OpenPauseMenu();
    }

    public void OnResumePress(){
        Unpause();
    }
    #endregion
}
