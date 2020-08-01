using Unity.Entities;
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
    private Animator animator;

    private EntityManager entityManager;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        timeTillSpawn = spawnIntervall;

        GameObject enemy = Instantiate(enemyPrefab, spawns[UnityEngine.Random.Range(0, spawns.Length)].transform.position, Quaternion.identity);
        enemy.GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
        enemy.GetComponent<NavMeshAgent>().stoppingDistance = entityManager.GetComponentData<RangeAttackData>(GetEntityOfComponent(typeof(RangeAttackData))).range;
    }

    private void Update()
    {
        return;
        timeTillSpawn -= Time.deltaTime;
        if (timeTillSpawn < 0)
        {
            timeTillSpawn = spawnIntervall;
            GameObject enemy = Instantiate(enemyPrefab, spawns[UnityEngine.Random.Range(0,spawns.Length)].transform.position, Quaternion.identity);
            enemy.GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
            enemy.GetComponent<NavMeshAgent>().stoppingDistance = entityManager.GetComponentData<RangeAttackData>(GetEntityOfComponent(typeof(RangeAttackData))).range;
            enemy.GetComponent<Animator>().SetBool("isWalking", true);
        }
    }

    private Entity GetEntityOfComponent(ComponentType componentType)
    {
        Unity.Collections.NativeArray<Entity> entities = entityManager.GetAllEntities();

        for (int i = 0; i < entities.Length; i++)
        {
            if (entityManager.HasComponent(entities[i], componentType))
            {
                return entities[i];
            }
        }

        entities.Dispose();

        return Entity.Null;
    }
}
