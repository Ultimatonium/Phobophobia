using System;
using Unity.Entities;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float rotationSpeed;
    [SerializeField]
    private float towerDistance = 10;
    [SerializeField]
    private GameObject towerPrefab;
    [SerializeField]
    private GameObject towerPlaceholderPrefab;

    private GameObject selectedTower;

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
    }

    private void Update()
    {
        transform.position += GetMoveDir();
        transform.rotation *= GetRotation();
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
        return Quaternion.Euler(0, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime, 0);
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
            tower.GetComponent<Animator>().SetBool("IsAttacking", false);
            Destroy(selectedTower);
        }
    }
}
