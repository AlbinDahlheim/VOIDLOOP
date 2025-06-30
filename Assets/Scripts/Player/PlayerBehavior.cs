using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehavior : MonoBehaviour
{
    public enum Direction
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        UP_LEFT
    }
    [HideInInspector] public Direction facingDirection = Direction.DOWN;

    public float plusShapedDeadzone = 0.15f;
    public float deadzone = 0.5f;

    // Player Input Actions
    public PlayerInputActions inputActions;
    private InputAction moveAction;

    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb2d;
    public AudioSource audioSource;

    public Vector2 LeftStickInput => leftStickInput;
    private Vector2 leftStickInput;

    [HideInInspector] public Vector2 movementForce;
    [HideInInspector] public Vector2 externalForce;

    public PlayerSheathed sheathedState = new PlayerSheathed();
    public PlayerUnsheathed unsheathedState = new PlayerUnsheathed();
    public PlayerStance stanceState = new PlayerStance();

    public PlayerState PreviousState => previousState;
    private PlayerState previousState;
    private PlayerState currentState;

    public void OnValidate()
    {
        sheathedState.OnValidate(this);
        unsheathedState.OnValidate(this);
        stanceState.OnValidate(this);
    }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        currentState = sheathedState;
    }

    private void OnEnable()
    {
        moveAction = inputActions.Player.Move;
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Update()
    {
        UpdateInput();

        currentState.Update();
    }

    private void FixedUpdate()
    {
        UpdatePhysics();

        currentState.FixedUpdate();
    }

    private void UpdateInput()
    {
        leftStickInput = moveAction.ReadValue<Vector2>();
        if (leftStickInput.magnitude <= deadzone)
            leftStickInput = Vector2.zero;
        if (Mathf.Abs(leftStickInput.x) <= plusShapedDeadzone)
            leftStickInput.x = 0.0f;
        if (Mathf.Abs(leftStickInput.y) <= plusShapedDeadzone)
            leftStickInput.y = 0.0f;
    }

    private void UpdatePhysics()
    {
        rb2d.linearVelocity = movementForce + externalForce;
        Debug.Log(rb2d.linearVelocity);
    }

    public void ChangeState(PlayerState targetState)
    {
        currentState.Exit();
        previousState = currentState;
        currentState = targetState;
        currentState.Enter();
    }

    public Direction GetDirectionOfVector(Vector2 direction)
    {
        direction = direction.normalized;

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, direction));
        angle = angle <= 0 ? angle * -1 : 360 - angle;
        int angleValue = (angle + 23) / 45 % 8;

        return (PlayerBehavior.Direction)angleValue;
    }

    public string GetDirectionName()
    {
        string directionName = facingDirection.ToString();

        directionName = directionName.Replace("RIGHT", "SIDE");
        directionName = directionName.Replace("LEFT", "SIDE");

        return directionName;
    }
}
