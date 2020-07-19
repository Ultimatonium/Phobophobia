using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private float spawnIntervall;
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private GameObject[] spawns;

    private float timeTillSpawn;

    private void Start()
    {
        timeTillSpawn = spawnIntervall;

        //GameObject enemy = Instantiate(enemyPrefab, spawns[UnityEngine.Random.Range(0, spawns.Length)].transform.position, Quaternion.identity);
        //enemy.GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
    }

    private void Update()
    {
        timeTillSpawn -= Time.deltaTime;
        if (timeTillSpawn < 0)
        {
            timeTillSpawn = spawnIntervall;
            GameObject enemy = Instantiate(enemyPrefab, spawns[UnityEngine.Random.Range(0,spawns.Length)].transform.position, Quaternion.identity);
            enemy.GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
        }
    }
}
