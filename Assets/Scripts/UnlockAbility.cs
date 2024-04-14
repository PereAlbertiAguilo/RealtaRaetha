using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAbility : MonoBehaviour, IDataPersistence
{
    bool wallJump = false;
    bool dash = false;
    bool dobleJump = false;

    private void Start()
    {
        if (wallJump && ablilityType == AblilityType.WallJump)
        {
            Destroy(this.gameObject);
        }
        if (dash && ablilityType == AblilityType.Dash)
        {
            Destroy(this.gameObject);
        }
        if (dobleJump && ablilityType == AblilityType.DobleJump)
        {
            Destroy(this.gameObject);
        }
    }

    public void SaveData(GameData data)
    {
        if (ablilityType == AblilityType.WallJump) data.wallJump = this.wallJump;
        if (ablilityType == AblilityType.Dash) data.dash = this.dash;
        if (ablilityType == AblilityType.DobleJump) data.dobleJump = this.dobleJump;
    }

    public void LoadData(GameData data)
    {
        this.wallJump = data.wallJump;
        this.dash = data.dash;
        this.dobleJump = data.dobleJump;
    }

    enum AblilityType { WallJump, Dash, DobleJump };

    [SerializeField] AblilityType ablilityType;

    void Unlock(PlayerController playerController)
    {
        switch (ablilityType)
        {
            case AblilityType.WallJump:

                wallJump = true;
                playerController.wallJump = wallJump;

                break;

            case AblilityType.Dash:

                dash = true;
                playerController.dash = dash;

                break;

            case AblilityType.DobleJump:

                dobleJump = true;
                playerController.dobleJump = dobleJump;

                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Unlock(other.GetComponent<PlayerController>());

            Destroy(this.gameObject);
        }
    }
}
