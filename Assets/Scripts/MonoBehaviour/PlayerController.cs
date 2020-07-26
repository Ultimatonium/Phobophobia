using System;
using Unity.Entities;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float characterRotationSpeed;
    [SerializeField]
    private float cameraRotationSpeed;
    private float cameraRadius;

    [SerializeField]
    private float damage;
    [SerializeField]
    private float towerDistance = 10;
    [SerializeField]
    private GameObject towerPrefab;
    [SerializeField]
    private GameObject towerPlaceholderPrefab;
    [SerializeField]
    private int towerCost;
    [SerializeField]
    private GameObject[] targets;

    private GameObject selectedTower;
    private GameObject characterCam;

    private Animator animator;

    private EntityManager entityManager;
    private Entity gameStateEntity;
    private Entity bank;

    private void Start()
    {
        GetComponent<Rigidbody>().mass = float.MaxValue;
        GetComponent<Rigidbody>().drag = float.MaxValue;

        animator = GetComponentInChildren<Animator>();

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<Camera>() != null)
            {
                characterCam = transform.GetChild(i).gameObject;
                break;
            }
        }
        cameraRadius = Math.Abs(characterCam.transform.localPosition.z);
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
    }

    private void Update()
    {
        if (gameStateEntity == Entity.Null) gameStateEntity = GetEntityOfComponent(typeof(GameStateData));
        if (bank == Entity.Null) bank = GetEntityOfComponent(typeof(MoneyData));
        if (gameStateEntity == Entity.Null) return;

        FlipPause();
        if (entityManager.GetComponentData<GameStateData>(gameStateEntity).gameState == GameState.Running)
        {
            Move();
            RotateCam();
            SelectTower();
            SetTowerPosition();
            ActiveTower();
            Attack();
        }
    }

    private void Move()
    {
        Vector3 moveDir = GetMoveDir();
        transform.rotation *= GetRotation();
        if (moveDir == Vector3.zero)
        {
            animator.SetBool("isRunning", false);
        }
        else
        {
            animator.SetBool("isRunning", true);
            transform.position += moveDir;
        }
    }

    private void Attack()
    {
        if (Input.GetMouseButtonDown(0) && selectedTower == null)
        {
            animator.SetTrigger("attack");
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

    private void FlipPause()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (entityManager.GetComponentData<GameStateData>(gameStateEntity).gameState == GameState.Running)
            {
                entityManager.SetComponentData(gameStateEntity, new GameStateData { gameState = GameState.Pause });
                Time.timeScale = 0;
            } else
            {
                entityManager.SetComponentData(gameStateEntity, new GameStateData { gameState = GameState.Running });
                Time.timeScale = 1;
            }
        }
    }

    private Vector3 GetMoveDir()
    {
        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveDir += transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir += -transform.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir += -transform.forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += transform.right;
        }

        return moveDir.normalized * moveSpeed * Time.deltaTime;
    }

    private Quaternion GetRotation()
    {
        return Quaternion.Euler(0, Input.GetAxis("Mouse X") * characterRotationSpeed * Time.deltaTime, 0);
    }

    private void RotateCam()
    {
        characterCam.transform.Translate(new Vector3(0, 0, cameraRadius));
        characterCam.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * cameraRotationSpeed * Time.deltaTime,0,0), Space.Self);
        characterCam.transform.Translate(new Vector3(0, 0, -cameraRadius));
    }
    private void SelectTower()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Destroy(selectedTower);
            selectedTower = null;
        }
        if (selectedTower != null) return;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            selectedTower = Instantiate(towerPlaceholderPrefab);
        }
    }

    private void SetTowerPosition()
    {
        if (selectedTower == null) return;
        selectedTower.transform.position = transform.position + transform.forward * towerDistance;
        selectedTower.transform.rotation = transform.rotation;
    }
    private void ActiveTower()
    {
        if (selectedTower == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            int currentMoney = entityManager.GetComponentData<MoneyData>(bank).money;
            if (currentMoney >= towerCost)
            {
                entityManager.SetComponentData<MoneyData>(bank, new MoneyData { money = currentMoney - towerCost });
                GameObject tower = Instantiate(towerPrefab, selectedTower.transform.position, selectedTower.transform.rotation);
                tower.GetComponent<Animator>().SetBool("isAttacking", false);
                Destroy(selectedTower);
            }else
            {
                Debug.Log("no cash");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Enemy") return;
        //if (collision.gameObject.transform.root.gameObject == gameObject.transform.root.gameObject) return;
        Entity enemyEntity = GetEntityOfGameObject(collision.gameObject);
        if (enemyEntity != Entity.Null)
        {
            entityManager.GetBuffer<HealthModifierBufferElement>(enemyEntity).Add(new HealthModifierBufferElement { value = -damage });
        }
    }

    private Entity GetEntityOfGameObject(GameObject gameObject)
    {
        Unity.Collections.NativeArray<Entity> entities = entityManager.GetAllEntities();

        for (int i = 0; i < entities.Length; i++)
        {
            //if (!entityManager.Exists(entities[i])) continue;
            if (!entityManager.HasComponent<Transform>(entities[i])) continue;
            if (entityManager.GetComponentObject<Transform>(entities[i]).gameObject == gameObject)
            {
                return entities[i];
            }
        }

        entities.Dispose();

        return Entity.Null;
    }
}
