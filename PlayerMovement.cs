using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private Transform movementOrienter;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController cc;
    private float verticalVelocity;
    public float VerticalVelocity => verticalVelocity;
    private Vector3 lastPosition;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float FlatSpeed { get; private set; }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }

    void Update()
    {
        bool grounded = cc.isGrounded;

        HandleMovement();
        HandleJumpAndGravity(grounded);
    }

    void HandleJumpAndGravity(bool grounded)
    {
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (grounded && input.ConsumeBufferedPress(PlayerInputNames.Jump))
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;
        cc.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;

        FlatSpeed = delta.magnitude / Time.deltaTime;
        lastPosition = transform.position;
    }

    void HandleMovement()
    {
        Vector2 move = input.GetVector2(PlayerInputNames.Move);
        float sprint = input.GetFloat(PlayerInputNames.Sprint);
        float speed = Mathf.Lerp(walkSpeed, runSpeed, sprint);

        Vector3 direction = movementOrienter.right * move.x + movementOrienter.forward * move.y;
        cc.Move(direction * speed * Time.deltaTime);
    }
}