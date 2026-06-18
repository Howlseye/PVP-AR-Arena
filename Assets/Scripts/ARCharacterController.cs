using UnityEngine;

public class ARCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f; // 3D world speed (can be overridden)
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

    private float attackTimer = 0f;

    private void Update()
    {
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        if (joystick == null) return;

        Vector2 input = joystick.InputDirection;
        float inputMagnitude = input.magnitude;

        // Fetch MoveSpeed from CharacterStats if available
        float currentMoveSpeed = moveSpeed;
        var stats = GetComponent<CharacterStats>();
        if (stats != null)
        {
            // Normalize moveSpeed from 0-100 to world speed. Assuming max speed is 5f at 100.
            // Scale by transform.localScale.x because in AR the character is tiny (e.g., 0.1 scale).
            float maxSpeed = 5f;
            currentMoveSpeed = (stats.MoveSpeed / 100f) * maxSpeed * transform.localScale.x; 
        }

        // Animate
        if (animator != null)
        {
            animator.speed = 1f; // Reset to normal speed for walking/idle
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
            transform.position += moveDirection * currentMoveSpeed * Time.deltaTime;

            // Rotate
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void ApplyAttackSpeed()
    {
        if (animator != null)
        {
            var stats = GetComponent<CharacterStats>();
            if (stats != null)
            {
                // Normalize atkSpeed from 0-100. Assume 50 is normal speed (1.0x).
                // So (atkSpeed / 50f) gives 1.0 at 50, 2.0 at 100.
                animator.speed = stats.AtkSpeed / 50f;
            }
        }
    }

    public void Attack()
    {
        if (animator != null)
        {
            ApplyAttackSpeed();
            animator.SetTrigger("Attack");
            attackTimer = 1.0f;
        }
    }

    public void Attack2()
    {
        if (animator != null)
        {
            ApplyAttackSpeed();
            animator.SetTrigger("Attack2");
            attackTimer = 1.0f;
        }
    }

    public void Shield()
    {
        if (animator != null)
        {
            animator.speed = 1f; // Reset speed for shield
            animator.SetTrigger("Shield");
        }
    }

    public bool IsAttacking()
    {
        return attackTimer > 0f;
    }
}
