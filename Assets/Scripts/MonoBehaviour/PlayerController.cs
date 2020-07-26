using System;
using Unity.Entities;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float damage;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float characterRotationSpeed;
    [SerializeField]
    private float cameraRotationSpeed;
    [SerializeField]
    private float towerDistance = 10;
    [SerializeField]
    private GameObject towerPrefab;
    [SerializeField]
    private GameObject towerPlaceholderPrefab;
    [SerializeField]
    private GameObject[] targets;

    private GameObject selectedTower;
    private GameObject characterCam;

    private Animator animator;

    private EntityManager entityManager;
    private Entity gameStateEntity;

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
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
    }

    private void Update()
    {
        if (gameStateEntity == Entity.Null) GetGameState();
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

    private void GetGameState()
    {
        Unity.Collections.NativeArray<Entity> entities = entityManager.GetAllEntities();

        for (int i = 0; i < entities.Length; i++)
        {
            if (entityManager.HasComponent<GameStateData>(entities[i]))
            {
                gameStateEntity = entities[i];
            }
        }

        entities.Dispose();
    }

    private void FlipPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        characterCam.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * cameraRotationSpeed * Time.deltaTime,0,0), Space.Self);
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
        if (Input.GetMouseButtonDown(0)) {
            GameObject tower = Instantiate(towerPrefab, selectedTower.transform.position, selectedTower.transform.rotation);
            tower.GetComponent<Animator>().SetBool("isAttacking", false);
            Destroy(selectedTower);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Enemy") return;
        //if (collision.gameObject.transform.root.gameObject == gameObject.transform.root.gameObject) return;
        Entity enemyEntity = GetEntityOfGameObject(collision.gameObject);
        entityManager.GetBuffer<HealthModifierBufferElement>(enemyEntity).Add(new HealthModifierBufferElement { value = -damage });
    }

    private Entity GetEntityOfGameObject(GameObject gameObject)
    {
        Unity.Collections.NativeArray<Entity> entities = entityManager.GetAllEntities();

        for (int i = 0; i < entities.Length; i++)
        {
            if (entityManager.HasComponent<Transform>(entities[i]))
            {
                if (entityManager.GetComponentObject<Transform>(entities[i]).gameObject == gameObject)
                {
                    return entities[i];
                }
            }
        }

        entities.Dispose();

        return Entity.Null;
    }
}
