using UnityEngine;

public class ARCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 0.5f; // Scaled down for AR space
    [SerializeField] private float rotationSpeed = 10f;

    [Header("References")]
    private PinePie.SimpleJoystick.JoystickController joystick;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetupJoystick(PinePie.SimpleJoystick.JoystickController assignedJoystick)
    {
        joystick = assignedJoystick;
    }

    private void Update()
    {
        if (joystick == null) return;

        Vector2 input = joystick.InputDirection;
        float inputMagnitude = input.magnitude;

        // Animate
        if (animator != null)
        {
            animator.SetFloat("Speed", inputMagnitude);
        }

        if (inputMagnitude > 0.05f)
        {
            // Calculate movement direction relative to the AR Camera's flat forward
            Transform camTransform = Camera.main.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;

            // Project vectors onto the XZ plane to keep movement horizontal
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * input.y + camRight * input.x;

            // Move
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Rotate
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void Attack2()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack2");
        }
    }

    public void Shield()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shield");
        }
    }
}
