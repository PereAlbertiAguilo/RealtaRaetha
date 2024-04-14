using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour, IDataPersistence
{
    [SerializeField] GameObject pasueMenu;

    [Header("Panels of the Pasue Menu")]
    [Space]
    [SerializeField] GameObject[] panels;

    [Header("UI that should be hiden while paused")]
    [Space]
    [SerializeField] GameObject otherUI;

    [Header("First Selected Buttons")]
    [Space]
    [SerializeField] GameObject firstPauseButton;

    public static PauseMenu instance;

    [HideInInspector] public bool isPaused = false;

    PlayerController playerController;

    GameObject lastSelectedGameObject;

    private void Awake()
    {
        playerController = transform.parent.GetChild(0).GetComponent<PlayerController>();

        instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
    }

    public void SaveData(GameData data)
    {
        data.lastScene = SceneManager.GetActiveScene().name;
    }

    public void LoadData(GameData data)
    {

    }

    void UpdateLastSelectedGameobject()
    {
        GameObject g = EventSystem.current.currentSelectedGameObject;

        if (g != lastSelectedGameObject && g != null)
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

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button1) && isPaused)
        {
            Resume();
        }
    }

    public void Resume()
    {
        isPaused = false;
        playerController.enabled = true;
        playerController.canTakeDamage = true;
        Time.timeScale = 1;
        pasueMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        SelectableUIElementsManager.ChangeCurrentSelectedGameObject(null);
        CloseAllPanels();
        UpdateOtherUI(true);
    }

    public void Pause()
    {
        isPaused = true;
        playerController.canTakeDamage = false;
        playerController.enabled = false;
        Time.timeScale = 0;
        pasueMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        SelectableUIElementsManager.ChangeCurrentSelectedGameObject(firstPauseButton);
        lastSelectedGameObject = firstPauseButton;
        UpdateOtherUI(false);

    }

    void StopAllAnimations(bool isStoped)
    {
        Animator[] allAnimators = FindObjectsOfType<Animator>(true);

        foreach (Animator a in allAnimators)
        {
            if (isStoped)
            {
                a.StopPlayback();
            }
            else
            {
                a.StartPlayback();
            }
        }
    }

    public void SaveQuit(string scene)
    {
        DataPersistenceManager.instance.SaveGame();
        StartCoroutine(ChangeScene.instance.ChangeSceneFunc(1.5f, scene, false, Vector2.zero));
    }

    void CloseAllPanels()
    {
        foreach(GameObject g in panels)
        {
            g.SetActive(false);
        }
    }

    void UpdateOtherUI(bool b)
    {
        otherUI.SetActive(b);
    }
}
