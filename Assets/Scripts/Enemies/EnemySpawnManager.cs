using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] int enemiesAmount = 4;
    [SerializeField] float maxTime = 60;
    public float currentTime;
    [SerializeField] GameObject enemy;

    public static int currentEnemies = 0;
    public int enemiesCount;

    public List<GameObject> enemies = new List<GameObject>();

    [Space]

    [SerializeField] float upLid;
    [SerializeField] float downLid;
    [SerializeField] float rightLid;
    [SerializeField] float leftLid;

    private void Start()
    {
        currentEnemies = 0;
        currentTime = 0;
    }

    private void Update()
    {
        enemiesCount = currentEnemies;

        Timer();

        if(enemies.Count == enemiesAmount)
        {
            if(currentEnemies <= 0)
            {
                currentTime = 0;
            }
        }
    }

    void Timer()
    {
        if(currentTime <= 0 && currentEnemies <= 0)
        {
            currentTime = maxTime;
            enemiesAmount = Random.Range(1, 9);
            enemies.Clear();
            StartCoroutine(SpawnEnemies());
        }

        if(currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
    }

    Vector2 RandomPos()
    {
        float x = Random.Range(leftLid, rightLid);
        float y = Random.Range(downLid, upLid);

        return new Vector2(x, y);
    }

    IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < enemiesAmount; i++)
        {
            GameObject instance = Instantiate(enemy, new Vector3(RandomPos().x, RandomPos().y, 0), Quaternion.identity);

            enemies.Add(instance);

            currentEnemies++;

            yield return new WaitForSeconds(.5f);

            if (instance == null)
            {
                enemies.Remove(instance);

                i--;

                currentEnemies--;
            }
        }
    }
}
