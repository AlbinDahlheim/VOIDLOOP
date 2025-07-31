using System.Collections;
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
    private InputAction swordAction;

    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer shadowRenderer;
    public SpriteRenderer auraRenderer;
    public Rigidbody2D rb2d;
    public AudioSource audioSource;

    public GameObject dustCloud;

    public Color defaultBodyColor;
    public Color defaultSwordColor;
    public Color defaultEyeColor;

    public Vector2 LeftStickInput => leftStickInput;
    private Vector2 leftStickInput;

    [HideInInspector] public Vector2 movementForce;
    public Vector2 InternalForce => internalForce;
    private Vector2 internalForce;
    public Vector2 ExternalForce => externalForce;
    private Vector2 externalForce;

    private float internalDecreaseIntensity;
    private float externalDecreaseIntensity;

    public PlayerSheathed sheathedState = new PlayerSheathed();
    public PlayerUnsheathed unsheathedState = new PlayerUnsheathed();
    public PlayerStance stanceState = new PlayerStance();
    public PlayerSwing swingState = new PlayerSwing();

    public PlayerState PreviousState => previousState;
    private PlayerState previousState;
    private PlayerState currentState;

    public void OnValidate()
    {
        sheathedState.OnValidate(this);
        unsheathedState.OnValidate(this);
        stanceState.OnValidate(this);
        swingState.OnValidate(this);
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

        swordAction = inputActions.Player.Sword;
        swordAction.Enable();
        swordAction.performed += SwordInput;
        swordAction.canceled += SwordInput;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        swordAction.Disable();
    }

    private void Update()
    {
        UpdateInput();
        ConstantlyUpdate();

        currentState.Update();
    }

    private void FixedUpdate()
    {
        UpdatePhysics();

        currentState.FixedUpdate();
    }

    private void ConstantlyUpdate() // Update that runs even when not in the current state
    {
        swingState.ConstantlyUpdate();
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

    private void SwordInput(InputAction.CallbackContext context)
    {
        currentState.SwordInput(context);
    }

    private void UpdatePhysics()
    {
        if (movementForce.sqrMagnitude >= internalForce.sqrMagnitude)
            rb2d.linearVelocity = movementForce + externalForce;
        else
            rb2d.linearVelocity = movementForce + internalForce + externalForce;

        internalForce -= internalForce.normalized * internalDecreaseIntensity * Time.deltaTime;
        if (internalForce.sqrMagnitude < 0.01f)
            internalForce = Vector2.zero;
    }

    public void ChangeState(PlayerState targetState)
    {
        currentState.Exit();
        previousState = currentState;
        currentState = targetState;
        currentState.Enter();
    }

    public void UpdateInternalForce(Vector2 force, float decreasePerSecond)
    {
        internalForce = force;
        internalDecreaseIntensity = decreasePerSecond;
    }

    public void UpdateExternalForce(Vector2 force, float decreasePerSecond)
    {
        externalForce = force;
        externalDecreaseIntensity = decreasePerSecond;
    }

    public Direction GetDirectionOfVector(Vector2 vector)
    {
        vector = vector.normalized;

        int angle = Mathf.RoundToInt(Vector2.SignedAngle(Vector2.up, vector));
        angle = angle <= 0 ? angle * -1 : 360 - angle;
        int angleValue = (angle + 23) / 45 % 8;

        return (PlayerBehavior.Direction)angleValue;
    }

    public Vector2 GetVectorOfDirection(Direction direction)
    {
        // 0 = 0
        // 1 = 45
        // 2 = 90
        // 3 = 135
        // 4 = 180
        // 5 = 225
        // 6 = 270
        // 7 = 315

        float angle = (int)direction * 45;
        Vector2 vector = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));

        return vector.normalized;
    }

    public string GetDirectionName()
    {
        string directionName = facingDirection.ToString();

        directionName = directionName.Replace("RIGHT", "SIDE");
        directionName = directionName.Replace("LEFT", "SIDE");

        return directionName;
    }

    public void SpawnDustCloud()
    {
        Vector3 offset = Vector2.up * -0.5f;
        Instantiate(dustCloud, transform.position + offset, transform.rotation);
    }

    public void Shake(int amount, float intensity)
    {
        StartCoroutine(ShakeAnimation(amount, intensity));
    }

    IEnumerator ShakeAnimation(int amount, float intensity)
    {
        Vector3 originalPos = spriteRenderer.gameObject.transform.localPosition;
        float directionMultiplier = spriteRenderer.flipX ? -1.0f : 1.0f;

        spriteRenderer.gameObject.transform.localPosition = originalPos + new Vector3(directionMultiplier * intensity, 0.0f, 0.0f);

        int currentShakes = 1;
        float timePassed = 0.0f;
        float durationPerShake = 0.05f;
        while (currentShakes <= amount)
        {
            yield return null;
            timePassed += Time.deltaTime;

            if (timePassed >= durationPerShake * currentShakes)
            {
                float currentDirection = currentShakes % 2 == 0 ? 1.0f : -1.0f;
                Vector3 offset = new Vector3(currentDirection * directionMultiplier * intensity, 0.0f, 0.0f);

                if (currentShakes >= amount) // Shakes are finished, return back to neutral
                    offset = Vector3.zero;

                spriteRenderer.gameObject.transform.localPosition = originalPos + offset;
                currentShakes++;
            }
        }
    }

    public void PaletteSwap(Color outlineColor, Color? bodyColor = null, Color? swordColor = null, Color? eyeColor = null)
    {
        spriteRenderer.material.SetColor("_OutlineTargetColor", outlineColor);
        spriteRenderer.material.SetColor("_BodyTargetColor", bodyColor ?? defaultBodyColor);
        spriteRenderer.material.SetColor("_SwordTargetColor", swordColor ?? defaultSwordColor);
        spriteRenderer.material.SetColor("_EyeTargetColor", eyeColor ?? defaultEyeColor);

        Color shadowColor = Color.Lerp(outlineColor, Color.black, 0.75f);
        shadowRenderer.material.SetColor("_TargetColor", shadowColor);

        auraRenderer.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, auraRenderer.color.a);
    }
}
