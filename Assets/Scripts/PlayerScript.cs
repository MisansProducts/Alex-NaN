using UnityEngine;

public class PlayerScript : MonoBehaviour {
    private GameScript gameScript;
    private Rigidbody2D rb;
    private Animator animator;
    private LayerMask groundLayer; 
    private Transform landingPos;

    [SerializeField] private float jumpForce; 
    [SerializeField] private float jumpHoldMultiplier; 
    [SerializeField] private float maxJumpTime; 
    [SerializeField] private int maxJumps; 
    private int jumpCount;  
    private float jumpTimeCounter;
    private bool isGrounded;
    private bool isJumping;
    private const float groundDistance = 0.2f;

    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundLayer = 1 << LayerMask.NameToLayer("Floor");
        landingPos = transform.Find("feet");
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Spike")) {
            gameScript.GameOver();
        }
    }

    void Update() {
        // Start a new jump if conditions are met
        if (Input.GetButtonDown("Jump") && (isGrounded || jumpCount < maxJumps)) {
            StartJump();
        }

        // Continue the jump while holding the button and within jump time
        if (Input.GetButton("Jump") && isJumping) {
            if (jumpTimeCounter > 0) {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpHoldMultiplier);
                jumpTimeCounter -= Time.deltaTime;
                animator.SetBool("isJumping", true);
            }
            else {
                isJumping = false;
                animator.SetBool("isJumping", false);
            }
        }

        // Stop the jump if the button is released
        if (Input.GetButtonUp("Jump")) {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }

        // Auto jump if the button is still held when landing
        if (isGrounded && Input.GetButton("Jump") && !isJumping) {
            StartJump();
        }
        
        if (gameScript.mode2 && Input.GetKeyDown(KeyCode.F) && gameScript.battery >= 1f / 3f) {
            gameScript.Mode2(1);
            gameScript.spotLightTime = 0;
            gameScript.spotLight.intensity = 1f;
            gameScript.battery -= 1f / 3f;
        }
    }

    void FixedUpdate() {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(landingPos.position, groundDistance, groundLayer);
        
        // Reset jump count when grounded
        if (isGrounded) {
            jumpCount = 0;
            animator.SetBool("isJumping", false);
        }
    }
    
    private void StartJump() {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        animator.SetBool("isJumping", true);

        // Increment jump count on every new jump initiation
        jumpCount++;
    }
}
