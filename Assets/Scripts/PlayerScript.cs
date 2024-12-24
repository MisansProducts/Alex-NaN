using UnityEngine;

public class PlayerScript : MonoBehaviour {
    private GameScript gameScript;
    private Rigidbody2D rb;
    private Animator animator;
    private LayerMask groundLayer; 
    private Transform landingPos;
    private float maxHeight = 11f;
    [SerializeField] public Animator flashAnimator;
    [SerializeField] public ParticleSystem flashParticles = default;
    [SerializeField] public AudioSource jump;
    [SerializeField] public AudioSource collect;
    [SerializeField] private float jumpForce; 
    [SerializeField] private float jumpHoldMultiplier; 
    [SerializeField] public float maxJumpTime; 
    [SerializeField] public int maxJumps; 
    private int jumpCount;  
    private float jumpTimeCounter;
    private bool isGrounded;
    private bool isJumping;
    private const float groundDistance = 0.2f;
    private float spawnX;
    public int extraJumpCount = 0;
    public int fuelCount = 0;

    [SerializeField] private GameObject shieldObject; // Reference to shield object
    private bool isShieldActive = false;

    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundLayer = 1 << LayerMask.NameToLayer("Floor");
        landingPos = transform.Find("feet");
        spawnX = transform.position.x;
        shieldObject.SetActive(false); // Initially disable the shield object
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Spike")) {
            if (isShieldActive) {
                // Shield is active, deactivate it after collision
                isShieldActive = false;
                shieldObject.SetActive(false);
            } else {
                gameScript.GameOver();
            }
        }
        if (collision.gameObject.CompareTag("OOB"))
            gameScript.GameOver();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Battery")) {
            Destroy(other.gameObject);
            gameScript.battery += 1 / 12f;
            collect.Play();
        }

        if (other.gameObject.CompareTag("Fuel")) {
            Destroy(other.gameObject);
            maxJumpTime += 0.2f;
            fuelCount++;
            collect.Play();
        }

        if (other.gameObject.CompareTag("ExtraJump")) {
            Destroy(other.gameObject);
            maxJumps++;
            extraJumpCount++;
            collect.Play();
        }

        if (other.gameObject.CompareTag("Shield")) {
            Destroy(other.gameObject);
            // Enable shield object and activate shield
            shieldObject.SetActive(true);
            isShieldActive = true;
            collect.Play();
        }
    }

    void Update() {
        // Start a new jump if conditions are met
        if (Input.GetButtonDown("Jump") && (isGrounded || jumpCount < maxJumps)) {
            StartJump();
            jump.Play();
        }

        // Continue the jump while holding the button and within jump time
        if (Input.GetButton("Jump") && isJumping) {
            if (jumpTimeCounter > 0) {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpHoldMultiplier);
                jumpTimeCounter -= Time.deltaTime;
                animator.SetBool("isJumping", true);
            } else {
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

        // MODE 2
        if (gameScript.mode2 && Input.GetKeyDown(KeyCode.F) && gameScript.battery >= GameScript.batteryDrainAmount) {
            flashAnimator.SetTrigger("flash");
            flashParticles.Play();
            gameScript.Flashed();
        }

        // if X position is changed somehow, slowly move back to spawn
        if (transform.position.x != spawnX) {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(spawnX, transform.position.y), 0.001f);
        }

        // limit the player's Y POS for the camera
        if (transform.position.y > maxHeight) {
            transform.position = new Vector2(transform.position.x, maxHeight);
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
