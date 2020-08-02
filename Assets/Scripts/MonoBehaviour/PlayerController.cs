using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private bool invertYAxis;
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float characterRotationSpeed;
    [SerializeField]
    private float cameraRotationSpeed;
    private float cameraRadius;

    [SerializeField]
    public float damage;
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
    //public GameObject characterCam;

    public GameObject characterCam { get; private set; }
    private Animator animator;
    private ParticleSystem feather;

    public EntityManager entityManager { get; private set; }
    private Entity gameStateEntity;
    private Entity bank;
    public Entity player { get; private set; }
    public List<GameObject> enemies { get; private set; }

    //private Transform cameraSpawnTransform;
    //private Transform playerSpawnTransform;
    public Vector3 spawnPostion { get; private set; }
    public Quaternion spawnRotation { get; private set; }

    private void Awake()
    {
        enemies = new List<GameObject>();
    }

    private void Start()
    {
        GetComponent<Rigidbody>().mass = float.MaxValue;
        //GetComponent<Rigidbody>().drag = float.MaxValue;

        animator = GetComponentInChildren<Animator>();
        feather = GetComponentInChildren<ParticleSystem>();

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
        //cameraSpawnTransform = characterCam.gameObject.transform;
        //playerSpawnTransform = transform;
        spawnPostion = transform.position;
        spawnRotation = transform.rotation;
    }

    private void Update()
    {
        if (gameStateEntity == Entity.Null) gameStateEntity = GetEntityOfComponent(typeof(GameStateData));
        if (bank == Entity.Null) bank = GetEntityOfComponent(typeof(MoneyData));
        if (player == Entity.Null) player = GetEntityOfComponent(typeof(PlayerTag));
        if (gameStateEntity == Entity.Null) return;
        if (bank == Entity.Null) return;
        if (player == Entity.Null) return;

        FlipPause();
        if (entityManager.GetComponentData<GameStateData>(gameStateEntity).gameState == GameState.Running)
        {
            if (entityManager.GetComponentData<HealthData>(player).health > 0)
            {
                Move();
                RotateCam();
                SelectTower();
                SetTowerPosition();
                ActiveTower();
                if (!Block())
                {
                    Attack();
                }
            }
            else
            {
                Respawn();
            }
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
            //GetComponent<CharacterController>().Move(moveDir);
            transform.position += moveDir;
        }
    }

    private bool Block()
    {
        if (Input.GetMouseButton(1) && selectedTower == null)
        {
            animator.SetBool("isBlocking", true);
            entityManager.SetComponentData<CombatStatusData>(player, new CombatStatusData { status = CombatStatus.Blocking });
            return true;
        }
        entityManager.SetComponentData<CombatStatusData>(player, new CombatStatusData { status = CombatStatus.NONE });
        animator.SetBool("isBlocking", false);
        return false;
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
            }
            else
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
        float invertion = 1;
        if (invertYAxis) invertion = -1;
        characterCam.transform.Translate(new Vector3(0, 0, cameraRadius));
        characterCam.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * cameraRotationSpeed * Time.deltaTime * invertion, 0, 0), Space.Self);
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
            }
            else
            {
                HUD.Instance.SetDisplayText("You need " + towerCost + " Axoloons");
                Debug.Log("no cash");
            }
        }
    }

    private void Respawn()
    {
        animator.SetTrigger("die");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Enemy") return;
        enemies.Add(collision.gameObject);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag != "Enemy") return;
        enemies.Remove(collision.gameObject);
    }
}
