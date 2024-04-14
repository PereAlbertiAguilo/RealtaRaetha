using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class SavePoint : MonoBehaviour, IDataPersistence
{
    PauseMenu pauseMenu;
    PlayerController pc;
    [SerializeField] Transform spawnPos;

    Vector2 savePos;

    float interpolationFactor = 0;

    public bool isSaving = false;
    public bool canSave = false;
    bool isRespawnSaved = false;

    private void Start()
    {
        pauseMenu = FindObjectOfType<PauseMenu>();
        pc = FindObjectOfType<PlayerController>();
    }

    public void SaveData(GameData data)
    {
        if (isRespawnSaved)
        {
            data.isRespawnSaved = this.isRespawnSaved;
            data.lastSpawnPoint = SceneManager.GetActiveScene().name;
            data.savePos = this.savePos;
        }
    }

    public void LoadData(GameData data)
    {

    }

    private void Update()
    {
        if (pc.verticalInput >= pc.xAxisThreshold && Mathf.Abs(pc.horizontalInput) < pc.xAxisThreshold && !pauseMenu.isPaused && pc.isGrounded)
        {
            if (canSave)
            {
                EnterSavePoint();
            }
        }

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) && isSaving)
        {
            ExitSavePoint();
        }

        if (isSaving)
        {
            if (canSave)
            {
                pc.transform.position = Vector3.MoveTowards(pc.transform.position, spawnPos.transform.position, interpolationFactor);

                interpolationFactor += .5f * Time.fixedDeltaTime;
            }
            else
            {
                pc.GetComponent<Rigidbody2D>().gravityScale = 0;

                pc.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                //pc.transform.position = spawnPos.transform.position;
            }
        }
    }

    void EnterSavePoint()
    {
        isRespawnSaved = true;
        pauseMenu.isPaused = true;

        pc.canMove = false;
        pc.currentHealth = pc.maxHealth;
        pc.currentMana = pc.maxMana;
        pc.UpdateHealthBar();
        pc.UpdateManaBar();
        pc._playerAnimator.Play("Player_Fall");

        isSaving = true;
        savePos = spawnPos.transform.position;

        interpolationFactor = 0;
    }

    void ExitSavePoint()
    {
        pc.GetComponent<Rigidbody2D>().gravityScale = pc.gravityScale;
        pauseMenu.isPaused = false;
        pc.canMove = true;
        isSaving = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canSave = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canSave = false;
        }
    }
}
