
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public GameObject enemy;
    public float spawnTime = 3f;
    public float startTime = 0f;
    public Transform[] spawnPoints;

    public static List<GameObject> enemyList = new List<GameObject>();


    void Start ()
    {
        InvokeRepeating ("Spawn", startTime, spawnTime);
    }


    void Spawn ()
    {
        if(playerHealth.currentHealth <= 0f)
        {
            return;
        }

        int spawnPointIndex = Random.Range (0, spawnPoints.Length);

        GameObject o = Instantiate (enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
        enemyList.Add(o);

        Debug.Log("Enemy spawned", gameObject);
    }
}
