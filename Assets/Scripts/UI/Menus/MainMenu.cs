using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour, IDataPersistence
{
    [SerializeField] GameObject continueButton;

    [SerializeField] SceneReference[] sceneReferences;

    [SerializeField] List<string> allScenes = new List<string>();

    public string lastScene;

    public GameObject lastSelectedGameObject;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 1;

        foreach (SceneReference s in sceneReferences)
        {
            allScenes.Add(s.ScenePath);
        }

        if (!DataPersistenceManager.instance.HasGameData())
        {
            continueButton.SetActive(false);
        }

        lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
    }

    void UpdateLastSelectedGameobject()
    {
        GameObject g = EventSystem.current.currentSelectedGameObject;

        if(g != lastSelectedGameObject)
        {
            lastSelectedGameObject = g;
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown && EventSystem.current.currentSelectedGameObject == null)
        {
            SelectableUIElementsManager.ChangeCurrentSelectedGameObject(lastSelectedGameObject);
        }

        UpdateLastSelectedGameobject();
    }

    public void SaveData(GameData data)
    {
        data.respawn = false;
    }

    public void LoadData(GameData data)
    {
        this.lastScene = data.lastScene;
    }

    public void Play(string scene)
    {
        DataPersistenceManager.instance.NewGame();

        foreach (string path in allScenes)
        {
            if (PlayerPrefs.HasKey(path + "StartPosX")) { PlayerPrefs.DeleteKey(path + "StartPosX"); }
            if (PlayerPrefs.HasKey(path + "StartPosY")) { PlayerPrefs.DeleteKey(path + "StartPosY"); }
        }

        PlayerPrefs.DeleteKey("mana");
        PlayerPrefs.DeleteKey("health");

        StartCoroutine(ChangeScene.instance.ChangeSceneFunc(1.5f, scene, false, Vector2.one));
        //SceneManager.LoadSceneAsync(scene);
    }

    public void Continue()
    {
        StartCoroutine(ChangeScene.instance.ChangeSceneFunc(1.5f, lastScene, false, Vector2.one));
        //SceneManager.LoadSceneAsync(lastScene);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }
}
