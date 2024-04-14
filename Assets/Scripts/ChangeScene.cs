using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour, IDataPersistence
{
    bool isRespawning = false;

    Animator animator;

    public static ChangeScene instance;

    private void Awake()
    {
        instance = this;

        animator = transform.GetChild(0).GetComponentInChildren<Animator>();
    }

    public void SaveData(GameData data)
    {
        data.respawn = this.isRespawning;
    }

    public void LoadData(GameData data)
    {

    }

    public IEnumerator ChangeSceneFunc(float delay, string sceneToGo, bool respawn, Vector2 nextSceneStartPos)
    {
        Time.timeScale = 1;

        animator.Play("FadeToBlack");

        Cursor.lockState = CursorLockMode.Locked;

        isRespawning = respawn;

        if (!respawn)
        {
            PlayerPrefs.SetFloat(sceneToGo.ToString() + "StartPosX", nextSceneStartPos.x);
            PlayerPrefs.SetFloat(sceneToGo.ToString() + "StartPosY", nextSceneStartPos.y);
        }

        yield return new WaitForSeconds(delay);

        SceneManager.LoadSceneAsync(sceneToGo);
    }
}
