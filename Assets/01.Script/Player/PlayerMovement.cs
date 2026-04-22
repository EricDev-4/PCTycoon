using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;

    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer spriteRenderer;
    [SerializeField ]private Sprite upSprite, downSprite;

    private void Start()
    {
        player = GetComponent<Player>();
        rb= GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (moveInput.magnitude > 0.1f)
        {
            spriteRenderer.sprite = moveInput.y > 0 ? upSprite : downSprite;
            player.foodSprite.sortingOrder = moveInput.y > 0 ? 0 : 101; 
        }
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
