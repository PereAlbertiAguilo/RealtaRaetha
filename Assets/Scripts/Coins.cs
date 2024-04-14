using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Coins : MonoBehaviour, IDataPersistence
{
    public static Coins instance;

    [SerializeField] TextMeshProUGUI coinsText;

    int coins = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PickUpCoin(0);
    }

    public void SaveData(GameData data)
    {
        data.coins = this.coins;
    }

    public void LoadData(GameData data)
    {
        this.coins = data.coins;
    }

    public void PickUpCoin(int coinsToAdd)
    {
        coins += coinsToAdd;

        coinsText.text = "" + coins;
    }
}
