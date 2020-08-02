using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private int increasePerWave;
    [SerializeField]
    private float waveSpawnInterval;
    [SerializeField]
    private GameObject[] spawns;

    private float timeTillSpawn;
    private int enemyPerWave;

    private void Start()
    {
        enemyPerWave = 0;
        timeTillSpawn = waveSpawnInterval;
    }

    private void Update()
    {
        timeTillSpawn -= Time.deltaTime;
        if (timeTillSpawn < 0)
        {
            timeTillSpawn = waveSpawnInterval;
            enemyPerWave += increasePerWave;
            int spawn = Random.Range(0, spawns.Length);
            for (int i = 0; i < enemyPerWave; i++)
            {
                Instantiate(enemyPrefab, spawns[spawn].transform.position, Quaternion.identity);
            }
        }
    }
}
