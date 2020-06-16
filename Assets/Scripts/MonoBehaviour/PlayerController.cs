using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float rotationSpeed;

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
    }

    private void Update()
    {
        transform.position += GetMoveDir();
        transform.rotation *= getRotation();
    }

    private Quaternion getRotation()
    {
        return Quaternion.Euler(0, Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime, 0);
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
}
