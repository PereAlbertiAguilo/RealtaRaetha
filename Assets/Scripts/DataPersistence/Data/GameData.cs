using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string lastScene;
    public string lastSpawnPoint;
    public Vector2 savePos;
    public bool isRespawnSaved;
    public bool respawn;

    public bool wallJump;
    public bool dash;
    public bool dobleJump;

    public int coins;

    // the values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public GameData() 
    {
        this.lastScene = "SP1";
        this.lastSpawnPoint = "SP1";
        this.savePos = new Vector2(32.5f, -26.06f);
        this.isRespawnSaved = false;
        this.respawn = false;

        this.wallJump = false;
        this.dash = false;
        this.dobleJump = false;

        this.coins = 0;
    }
}
