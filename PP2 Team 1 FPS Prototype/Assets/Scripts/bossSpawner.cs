using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] bossesToSpawn; // Array of bosses
    [SerializeField] Transform[] spawnPos;

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
        isSpawning = true;
        int arrayPos = Random.Range(0, spawnPos.Length);

        // Spawn the current boss
        GameObject spawnedBoss = Instantiate(bossesToSpawn[currentBossIndex], spawnPos[arrayPos].position, spawnPos[arrayPos].rotation);
        bossAI spawnedBossAI = spawnedBoss.GetComponent<bossAI>();

        // Check if the bossAI component exists before subscribing to the event
        if (spawnedBossAI != null)
        {
            spawnedBossAI.OnBossDeath += OnBossDeath;
        }
        else
        {
            Debug.LogError("bossAI component not found on the spawned boss!");
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
            Debug.Log("All bosses defeated!");
        }
    }
}
