using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    private GameObject selectedTower;
    private GameObject characterCam;

    private void Start()
    {
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
        transform.position += GetMoveDir();
        transform.rotation *= GetRotation();
        RotateCam();
        SelectTower();
        SetTowerPosition();
        ActiveTower();
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
}
