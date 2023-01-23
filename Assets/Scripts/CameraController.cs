using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 50.0f;
    public float rotationSpeed = 200.0f;
    public float damping = 10.0f;

    private CharacterController characterController;
    private Vector2 turn;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Rotation
        turn.x += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        turn.y += Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        turn.y = Mathf.Clamp(turn.y, -90f, 90f);

        // Smooth the rotation
        Quaternion targetRotation = Quaternion.Euler(-turn.y, turn.x, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * damping);
    }
}
