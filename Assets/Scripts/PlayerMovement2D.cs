using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float animationTurnBuffer = 0.1f;
    [SerializeField] private float stateCrossFadeDuration = 0.04f;

    private static readonly int MoveXHash = Animator.StringToHash("moveX");
    private static readonly int MoveYHash = Animator.StringToHash("moveY");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lastFacing = Vector2.down;
    private string currentAnimState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ReadInput();
        UpdateAnimationParameters();
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void ReadInput()
    {
        var rawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveInput = rawInput.sqrMagnitude > 1f ? rawInput.normalized : rawInput;

        if (moveInput.sqrMagnitude > 0f)
        {
            lastFacing = ResolveFacingDirection(moveInput);
        }
    }

    private Vector2 ResolveFacingDirection(Vector2 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absX > absY + animationTurnBuffer)
        {
            return direction.x > 0f ? Vector2.right : Vector2.left;
        }

        if (absY > absX + animationTurnBuffer)
        {
            return direction.y > 0f ? Vector2.up : Vector2.down;
        }

        return lastFacing;
    }

    private void UpdateAnimationParameters()
    {
        bool isMoving = moveInput.sqrMagnitude > 0f;
        Vector2 directionForAnimation = isMoving ? ResolveFacingDirection(moveInput) : lastFacing;

        animator.SetFloat(MoveXHash, directionForAnimation.x);
        animator.SetFloat(MoveYHash, directionForAnimation.y);
        animator.SetBool(IsMovingHash, isMoving);
    }

    private void UpdateAnimationState()
    {
        bool isMoving = animator.GetBool(IsMovingHash);
        float moveX = animator.GetFloat(MoveXHash);
        float moveY = animator.GetFloat(MoveYHash);

        string nextState;

        if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
        {
            nextState = moveX > 0f
                ? (isMoving ? "Walk_Right" : "Idle_Right")
                : (isMoving ? "Walk_Left" : "Idle_Left");
        }
        else
        {
            nextState = moveY > 0f
                ? (isMoving ? "Walk_Back" : "Idle_Back")
                : (isMoving ? "Walk_Front" : "Idle_Front");
        }

        if (nextState == currentAnimState)
        {
            return;
        }

        animator.CrossFadeInFixedTime(nextState, stateCrossFadeDuration);
        currentAnimState = nextState;
    }
}
