using Unity.Entities;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private float spawnIntervall;
    [SerializeField]
    private GameObject[] spawns;

    private float timeTillSpawn;

    private void Start()
    {
        timeTillSpawn = spawnIntervall;
    }

    private void Update()
    {
        timeTillSpawn -= Time.deltaTime;
        if (timeTillSpawn < 0)
        {
            timeTillSpawn = spawnIntervall;
            Instantiate(enemyPrefab, spawns[UnityEngine.Random.Range(0,spawns.Length)].transform.position, Quaternion.identity);
        }
    }
}
