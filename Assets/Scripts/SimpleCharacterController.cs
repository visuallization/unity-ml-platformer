using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    [Tooltip("Maximum slope the character can jump on")]
    [Range(5f, 60f)]
    public float slopeLimit = 45f;
    [Tooltip("Move speed in meters/seconds")]
    public float moveSpeed = 5f;
    [Tooltip("Turn speed in degrees/seconds, left (+) or right (-)")]
    public float turnSpeed = 300f;
    [Tooltip("Upward speed to apply when jumping in meters/seconds")]
    public float jumpSpeed = 4f;
    [Tooltip("Whether the character can jump")]
    public bool allowJump = true;

    // Checks if the player is jumping or falling but also if the player is on a slope greater then slopeLimit
    public bool IsGrounded { get; private set; }
    // Expects a value from -1 to 1 and controls forward movement.
    // -1 is full speed backward, +1 is full speed forward, 0 is no forward input.
    public float ForwardInput { get; set; }
    // Expects a value from -1 to 1 and controls turning.
    // -1 is full speed to the right, 1 is full speed to the left, 0 is no turn.
    public float TurnInput { get; set; }
    // Takes a true/false value indicating whether to jump.
    public bool JumpInput { get; set; }

    new private Rigidbody rigidbody;
    private CapsuleCollider capsuleCollider;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ProcessActions();
    }

    /// <summary>
    /// Checks whether the character is on the ground and updates <see cref="IsGrounded"/>
    /// </summary>
    private void CheckGrounded()
    {
        IsGrounded = false;
        float capsuleHeight = Mathf.Max(capsuleCollider.radius * 2f, capsuleCollider.height);
        Vector3 capsuleBottom = transform.TransformPoint(capsuleCollider.center - Vector3.up * capsuleHeight / 2f);
        float radius = transform.TransformVector(capsuleCollider.radius, 0f, 0f).magnitude;
        Ray ray = new Ray(capsuleBottom + transform.up * .01f, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, radius * 5f))
        {
            float normalAngle = Vector3.Angle(hit.normal, transform.up);
            if (normalAngle < slopeLimit)
            {
                float maxDistance = radius / Mathf.Cos(Mathf.Deg2Rad * normalAngle) - radius + .02f;
                if (hit.distance < maxDistance)
                {
                    IsGrounded = true;
                }
            }
        }
    }

    private void ProcessActions()
    {
        if (TurnInput != 0f)
        {
            float angle = Mathf.Clamp(TurnInput, -1f, 1f) * turnSpeed;
            transform.Rotate(Vector3.up, Time.fixedDeltaTime * angle);
        }

        if (IsGrounded)
        {
            rigidbody.velocity = Vector3.zero;
            Debug.Log(JumpInput);

            // Jump
            if (JumpInput && allowJump)
            {
                rigidbody.velocity += Vector3.up * jumpSpeed;
            }

            // Move forward/backward
            rigidbody.velocity += transform.forward * Mathf.Clamp(ForwardInput, -1f, 1f) * moveSpeed;
        }
        else
        {
            // If player wants to move forward/backward while jumping/falling
            if (!Mathf.Approximately(ForwardInput, 0f))
            {
                Vector3 verticalVelocity = Vector3.Project(rigidbody.velocity, Vector3.up);
                rigidbody.velocity = verticalVelocity + transform.forward * Mathf.Clamp(ForwardInput, -1f, 1f) * moveSpeed / 2f;
            }
        }
    }
}
