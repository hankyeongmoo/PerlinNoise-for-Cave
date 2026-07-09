using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 10f;
    public float rotationSpeed = 100f;
    
    private float appliedSpeed;


    void Update()
    {
        MoveCamera();
        CameraRotation();
    }

    void MoveCamera()
    {
        // moving from WASD
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * appliedSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * appliedSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * appliedSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * appliedSpeed * Time.deltaTime);
        }

        // Vertical movement from Space and Left Ctrl
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * appliedSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(Vector3.down * appliedSpeed * Time.deltaTime);
        }

        // running with Left Shift
        if (Input.GetKey(KeyCode.LeftShift))
        {
            appliedSpeed = runSpeed;
        }
        else
        {
            appliedSpeed = moveSpeed;
        }
    }

    void CameraRotation()
    {
        // turing camera with arrow keys
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, -rotationSpeed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationSpeed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(-rotationSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(rotationSpeed * Time.deltaTime, 0, 0));
        }
    }
}
