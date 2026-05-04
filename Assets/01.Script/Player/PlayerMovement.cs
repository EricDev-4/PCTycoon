using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;

    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] private FloatingCharacterMotion movementMotion;
    [SerializeField] private Sprite upSprite, downSprite;

    private void Start()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();

        if (movementMotion == null)
        {
            movementMotion = GetComponentInChildren<FloatingCharacterMotion>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = movementMotion != null
                ? movementMotion.GetComponent<SpriteRenderer>()
                : GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        if (moveInput.magnitude > 0.1f)
        {
            spriteRenderer.sprite = moveInput.y > 0 ? upSprite : downSprite;
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                spriteRenderer.flipX = moveInput.x > 0f;
            }
            player.foodSprite.sortingOrder = moveInput.y > 0 ? 0 : 101;
        }

        movementMotion?.SetMovement(moveInput * moveSpeed);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
