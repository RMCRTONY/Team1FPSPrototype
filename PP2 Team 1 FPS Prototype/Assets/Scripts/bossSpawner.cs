using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossSpawner : MonoBehaviour
{
    [Header("---------- Spawner Settings ----------")]
    [SerializeField] GameObject[] bossesToSpawn; // Array of bosses
    [SerializeField] Transform[] spawnPos;
    [SerializeField] float spawnRate = 1f;

    //[Header("---------- Main Camera ----------")]
    //[SerializeField] private cameraController cameraController;  // Reference to the camera controller

    private int currentBossIndex = 0;
    private bool isSpawning = false;
    private bool startSpawning = false;

    void Start()
    {
        StartCoroutine(SpawnBoss());
        gameManager.instance.updateGameGoal(bossesToSpawn.Length); // Update goal with the total number of bosses
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (startSpawning && !isSpawning)
    //    {
    //        StartCoroutine(SpawnBoss());
    //    }
    //}

    IEnumerator SpawnBoss()
    {
        yield return new WaitForSeconds(spawnRate);
        isSpawning = true;
        int arrayPos = Random.Range(0, spawnPos.Length);

        // Spawn the current boss
        GameObject spawnedBoss = Instantiate(bossesToSpawn[currentBossIndex], spawnPos[arrayPos].position, spawnPos[arrayPos].rotation);

        // Handle OnBossDeath event based on the type of AI component
        bossAI bossAI = spawnedBoss.GetComponent<bossAI>();
        if (bossAI != null)
        {
            bossAI.OnBossDeath += OnBossDeath;
        }
        else
        {
            bossBMAI bossBMAI = spawnedBoss.GetComponent<bossBMAI>();
            if (bossBMAI != null)
            {
                bossBMAI.OnBossDeath += OnBossDeath; // Assuming bossBMAI also has OnBossDeath event
            }
            else
            {
                //Debug.LogError("No recognized boss AI component found on the spawned boss!");
            }
        }

        yield return null; // Wait a frame to let the boss initialize
        isSpawning = false; // Only set to false after boss is spawned
    }

    void OnBossDeath()
    {
        currentBossIndex++; // Move to the next boss
        if (currentBossIndex < bossesToSpawn.Length)
        {
            StartCoroutine(SpawnBoss()); // Start spawning the next boss
        }
        else
        {
            // All bosses defeated - do something here (e.g., end the level)
            //Debug.Log("All bosses defeated!");
        }
    }
}
