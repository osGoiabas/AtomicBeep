using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        if (GameInput.WasPausePressed){
            if (!isPaused)
                Pause();
            else
                Unpause();
        }
    }

    public void Pause() {
        isPaused = true;
        Time.timeScale = 0f;
        //GameInput.PlayerInput.SwitchCurrentActionMap("UI");
        SerializeJson();
        OpenPauseMenu();
    }

    public void Unpause(){
        isPaused = false;
        Time.timeScale = 1f;
        //GameInput.PlayerInput.SwitchCurrentActionMap("Player");
        CloseAllMenus();
    }

    public void BackToMainMenuPress(){
        Debug.Log("BackToMainMenuPress");
        SceneTransitioner.Instance.LoadScene("MainMenu");
    }

    #region Open/Close Menus

    private void OpenPauseMenu(){
        _pauseMenuCanvas.SetActive(true);
        _settingsMenuCanvas.SetActive(false);
        Debug.Log("OpenPauseMenu");

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

    public void OnBackToMainMenuPress(){
        BackToMainMenuPress();
    }

    #endregion

    #region Serialize Data (Save Load)

    //private PlayerStats PlayerStats = new PlayerStats();
    private IDataService DataService = new JsonDataService();
    private bool EncryptionEnabled = false;

    public void SerializeJson()
    {
        if (DataService.SaveData("/player-stats.json", ItemManager.radsCollected, EncryptionEnabled))
        {
            try
            {
                int data = DataService.LoadData<int>("/player-stats.json", EncryptionEnabled);
            }
            catch
            {
                Debug.LogError($"Could not read file! Show something on the UI here!");
            }
        }
        else
        {
            Debug.LogError("Could not save file! Show something on the UI about it!");
        }
    }

    public void ToggleEncryption(bool EncryptionEnabled)
    {
        this.EncryptionEnabled = EncryptionEnabled;
    }

    public void ClearData()
    {
        string path = Application.persistentDataPath + "/player-stats.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            //InputField.text = "Loaded data goes here";
        }
    }

    #endregion
}
