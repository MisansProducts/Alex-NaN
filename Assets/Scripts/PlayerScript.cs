using UnityEngine;

public class PlayerScript : MonoBehaviour {
    [SerializeField] public float jumpForce; // Base jump force
    [SerializeField] public float jumpHoldMultiplier; // Multiplier to extend jump height
    [SerializeField] public float maxJumpTime; // Max time the jump can be held

    private GameScript gameScript;
    private LayerMask groundLayer; // Layer mask for ground detection
    private Transform landingPos;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;
    private const float groundDistance = 0.2f;

    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        rb = GetComponent<Rigidbody2D>();
        groundLayer = 1 << LayerMask.NameToLayer("Floor");
        landingPos = transform.Find("feet");
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Spike")) {
            gameScript.GameOver();
            // Destroy(gameObject);
        }
    }

    void Update() {
        // Start the jump if the button is pressed and the player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded) {
            StartJump();
        }

        // Continue the jump while holding the button and within allowed jump time
        if (Input.GetButton("Jump") && isJumping) {
            if (jumpTimeCounter > 0) {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpHoldMultiplier);
                jumpTimeCounter -= Time.deltaTime;
            }
            else {
                isJumping = false;
            }
        }

        // Stop the jump if the button is released
        if (Input.GetButtonUp("Jump")) {
            isJumping = false;
        }

        // Auto jump if the button is still held when landing
        if (isGrounded && Input.GetButton("Jump") && !isJumping) {
            StartJump();
        }
    }

    void FixedUpdate() {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(landingPos.position, groundDistance, groundLayer);
    }
    
    private void StartJump() {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
}
