using UnityEngine;

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
    public Direction FacingDirection => facingDirection;
    private Direction facingDirection = Direction.DOWN; 

    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb2d;
    public AudioSource audioSource;

    public PlayerSheathed sheathedState = new PlayerSheathed();
    public PlayerUnsheathed unsheathedState = new PlayerUnsheathed();
    public PlayerStance stanceState = new PlayerStance();

    public PlayerState CurrentState => currentState;
    private PlayerState currentState;

    public PlayerState PreviousState => previousState;
    private PlayerState previousState;

    public void OnValidate()
    {
        sheathedState.OnValidate(this);
        unsheathedState.OnValidate(this);
        stanceState.OnValidate(this);
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void ChangeState(PlayerState targetState)
    {
        currentState.Exit();
        previousState = currentState;
        currentState = targetState;
        currentState.Enter();
    }
}
