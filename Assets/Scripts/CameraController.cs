using UnityEngine;

public class CameraController : MonoBehaviour
{
    const int MIN_MOVE_SPEED = 10;
    const int MAX_MOVE_SPEED = 200;

    [Range(MIN_MOVE_SPEED, MAX_MOVE_SPEED)]
    public float moveSpeed = 50.0f;
    public float rotationSpeed = 200.0f;
    public float damping = 10.0f;

    private float moveSpeedSensitivity = 10.0f;
    private CharacterController characterController;
    private Vector3 turn;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Adjustable Move Speed
        moveSpeed += Input.GetAxis("Mouse ScrollWheel") * moveSpeedSensitivity;
        moveSpeed = Mathf.Clamp(moveSpeed, MIN_MOVE_SPEED, MAX_MOVE_SPEED);

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
        Quaternion targetRotation = Quaternion.Euler(-turn.y, turn.x, turn.z);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * damping);
    }
}
