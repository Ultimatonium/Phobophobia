using System;
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

    [SerializeField] private PlayerAudioData playerAudio;
    [SerializeField] private float footstepSpeed = 0.275f;
    [SerializeField] private TowerAudioData towerAudio;

    private FMOD.Studio.EventInstance heartbeating;
    private FMOD.Studio.EventInstance movingTower;
    private enum CURRENT_TERRAIN {Metal, Sand, Wood, Untagged};
    private CURRENT_TERRAIN currentTerrain;
    
    private float timer = 0f;
    private bool isWalking = false;
    private bool alreadyBeating = false;
    private bool alreadyDead = false;
    private bool waveAlreadyHere = false;
    private bool movingTowerSound = false;

  private GameObject selectedTower;

    public GameObject characterCam { get; private set; }
    private Animator animator;
    private ParticleSystem feather;

    public EntityManager entityManager { get; private set; }
    private Entity gameStateEntity;
    private Entity bank;
    public Entity player { get; private set; }
    public List<GameObject> enemies { get; private set; }

    public Vector3 spawnPostion { get; private set; }
    public Quaternion spawnRotation { get; private set; }

    private GameObject spawner;
    private float timeTilWave;

    private void Awake()
    {
        enemies = new List<GameObject>();
    }

    private void Start()
    {
        GetComponent<Rigidbody>().mass = float.MaxValue;

        animator = GetComponentInChildren<Animator>();
        feather = GetComponentInChildren<ParticleSystem>();

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        spawner = GameObject.FindWithTag("Spawner");
        timeTilWave = spawner.GetComponent<Spawner>().WaveSpawnInterval;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<Camera>() != null)
            {
                characterCam = transform.GetChild(i).gameObject;
                break;
            }
        }
        cameraRadius = Math.Abs(characterCam.transform.localPosition.z);

        spawnPostion = transform.position;
        spawnRotation = transform.rotation;

        PlayOneShotRandomEvent(new int[] {1, 2}, "/Phobo/Vox/StartGameResume/", false);
    }

    private void PlayFootstepAudio()
    {
      if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 10f))
      {
        switch(hitInfo.collider.tag)
        {
          case "Metal":
            currentTerrain = CURRENT_TERRAIN.Metal;
            FMODUnity.RuntimeManager.PlayOneShot(playerAudio.FootstepsMetal);
            break;
          case "Sand":
            currentTerrain = CURRENT_TERRAIN.Sand;
            FMODUnity.RuntimeManager.PlayOneShot(playerAudio.FootstepsSand);
            break;
          case "Wood":
            currentTerrain = CURRENT_TERRAIN.Wood;
            FMODUnity.RuntimeManager.PlayOneShot(playerAudio.FootstepsWood);
            break;
          default: //If there is no tag, use what probably is most adequate.
            currentTerrain = CURRENT_TERRAIN.Untagged;
            FMODUnity.RuntimeManager.PlayOneShot(playerAudio.FootstepsSand);
            break;
        }
      }

      //Debug.Log("Terrain: " + currentTerrain);
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

                if(animator.GetBool("hitted"))
                  FMODUnity.RuntimeManager.PlayOneShot(playerAudio.Hitsounds); //Event Macro - Cooldown: 240 ms!

                if(entityManager.GetComponentData<HealthData>(player).health < 2f)
                {
                  if(!alreadyBeating) //If the instance is already playing, then calling "start()" will restart the event.
                  { 
                    heartbeating = FMODUnity.RuntimeManager.CreateInstance(playerAudio.Heartbeats);
                    heartbeating.start();
                    alreadyBeating = true;
                  }
                }
                else
                {
                  heartbeating.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                  heartbeating.release();
                  alreadyBeating = false;
                }

                alreadyDead = false;
            }
            else
            {
                Respawn();
            }
        }

        if(isWalking)
        {
          if(timer > footstepSpeed)
          {
            PlayFootstepAudio();
            timer = 0f;
          }

          timer += Time.deltaTime;
        }

        timeTilWave -= Time.deltaTime;
        if(timeTilWave <= 0f && !waveAlreadyHere)
        {
          PlayOneShotRandomEvent(new int[] {1, 2, 3}, "/Phobo/Vox/StartofWave/", false);
          waveAlreadyHere = true;
        }
    }

    private void Move()
    {
        Vector3 moveDir = GetMoveDir();
        transform.rotation *= GetRotation();
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private bool Block()
    {
        if (Input.GetMouseButton(1) && selectedTower == null)
        {
            animator.SetBool("isBlocking", true);
            entityManager.SetComponentData<CombatStatusData>(player, new CombatStatusData { status = CombatStatus.Blocking });
            if(animator.GetBool("hitted"))
              FMODUnity.RuntimeManager.PlayOneShot(playerAudio.PillowBlock); //Event Macro - Cooldown: 240 ms!
            return true;
        }
        entityManager.SetComponentData<CombatStatusData>(player, new CombatStatusData { status = CombatStatus.NONE });
        animator.SetBool("isBlocking", false);
        return false;
    }

    private void Attack()
    {
        if (Input.GetMouseButtonDown(0) && selectedTower == null && !animator.GetBool("attack"))
        {
            animator.SetTrigger("attack");
            FMODUnity.RuntimeManager.PlayOneShot(playerAudio.PillowAttack);
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
                Pause();
            }
            else
            {
                Resume();
            }
        }
    }

    private void Pause()
    {
        entityManager.SetComponentData(gameStateEntity, new GameStateData { gameState = GameState.Pause });
        Time.timeScale = 0;
        PlayOneShotRandomEvent(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}, "/Phobo/Vox/RandomCommentary/", false);
    }

    public void Resume()
    {
        entityManager.SetComponentData(gameStateEntity, new GameStateData { gameState = GameState.Running });
        Time.timeScale = 1;
        PlayOneShotRandomEvent(new int[] {1, 2}, "/Phobo/Vox/StartGameResume/", false);
    }

    private Vector3 GetMoveDir()
    {
        Vector3 moveDir = Vector3.zero;
        animator.SetFloat("Move", 0);
        animator.SetFloat("Strafe", 0);
        isWalking = false;

        if (Input.GetKey(KeyCode.W))
        {
            moveDir += transform.forward;
            animator.SetFloat("Move", 1);
            isWalking = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir += -transform.right;
            animator.SetFloat("Strafe", -1);
            isWalking = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir += -transform.forward;
            animator.SetFloat("Move", -1);
            isWalking = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += transform.right;
            animator.SetFloat("Strafe", 1);
            isWalking = true;
        }

        return moveDir.normalized;
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
        if(!movingTowerSound)
        { 
          movingTower = FMODUnity.RuntimeManager.CreateInstance(towerAudio.TowerMove);
          movingTower.start();
          movingTower.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(selectedTower, selectedTower.GetComponent<Rigidbody>()));
          movingTowerSound = true;
        }
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

                movingTower.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                movingTower.release();
                movingTowerSound = false;

                FMODUnity.RuntimeManager.PlayOneShot(towerAudio.Build);
                PlayOneShotRandomEvent(new int[] {1, 2, 3, 4}, "/Phobo/Vox/PlacingTower/", false);
            }
            else
            {
                HUD.Instance.SetDisplayText("You need " + towerCost + " Axoloons!");
            }
        }
    }

    private void Respawn()
    {
      if(!alreadyDead)
      {
        PlayOneShotRandomEvent(new int[] { 1, 2, 3}, "/Phobo/Vox/Dead/", false);
        animator.SetTrigger("die");
        FMODUnity.RuntimeManager.PlayOneShot(playerAudio.Respawn);
        alreadyDead = true;
      }
    }

    //"eventPath" is also optional, but if you specify one, enclose it in slashes - e.g. "/eventPath/"!
    private void PlayOneShotRandomEvent(int[] eventNames, string eventPath = "/", bool debug = false)
    {
      FMODUnity.RuntimeManager.PlayOneShot("event:" + eventPath + eventNames[UnityEngine.Random.Range(0, eventNames.Length)]);
      if(debug)
        Debug.Log("event:" + eventPath + eventNames[UnityEngine.Random.Range(0, eventNames.Length)]);
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
