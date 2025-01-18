using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlayerScript : MonoBehaviour {
    private GameScript gameScript;
    private Rigidbody2D rb;
    private Animator animator;
    private LayerMask groundLayer; 
    private Transform landingPos;
    private float maxHeight = 11f;
    [SerializeField] public Animator flashAnimator;
    [SerializeField] public ParticleSystem flashParticles = default;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button flashButton;
    [SerializeField] private float jumpForce; 
    [SerializeField] private float jumpHoldMultiplier; 
    [SerializeField] public float maxJumpTime; 
    [SerializeField] public int maxJumps; 
    [SerializeField] public TextMeshProUGUI savedJumpUI;
    [SerializeField] public Image fuelBar;
    private int jumpCount; 
    private int availableJump; 
    private float jumpTimeCounter;
    private bool isGrounded;
    private bool isGroundedLock; // prevents spamming audio
    private bool isJumping;
    private bool keyDOWN, keyHOLD, keyUP, keyFLASH;
    private const float groundDistance = 0.2f;
    private float spawnX;
    public int extraJumpCount = 0;
    public int fuelCount = 0;

    [SerializeField] private GameObject shieldObject; // Reference to shield object
    private bool isShieldActive = false;
    private bool isMobile = false;

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
                SoundEffects.Instance.PlaySound(SoundEffects.Instance.shieldPop);
            } else {
                gameScript.GameOver();
            }
        }
        if (collision.gameObject.CompareTag("OOB"))
            gameScript.GameOver();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Battery")) {
            other.gameObject.transform.position = new Vector3(-10, 5, 0);
            gameScript.batteryCharging += 1f;
            SoundEffects.Instance.PlaySound(SoundEffects.Instance.pickup);
        }
        if (other.gameObject.CompareTag("Shield")) {
            other.gameObject.transform.position = new Vector3(-10, 6, 0);
            // Enable shield object and activate shield
            shieldObject.SetActive(true);
            isShieldActive = true;
            SoundEffects.Instance.PlaySound(SoundEffects.Instance.pickup);
        }
        if (other.gameObject.CompareTag("Fuel")) {
            other.gameObject.transform.position = new Vector3(-10, 7, 0);
            maxJumpTime += 0.2f;
            fuelCount++;
            SoundEffects.Instance.PlaySound(SoundEffects.Instance.pickup);
        }

        if (other.gameObject.CompareTag("ExtraJump")) {
            other.gameObject.transform.position = new Vector3(-10, 8, 0);
            maxJumps++;
            extraJumpCount++;
            SoundEffects.Instance.PlaySound(SoundEffects.Instance.pickup);
        }

        
    }

    void Start() {
        isGroundedLock = false; // doesn't work; should not play when game starts
        isMobile = Application.isMobilePlatform;
        // Show/hide button based on platform
        if (jumpButton != null) {
            jumpButton.gameObject.SetActive(isMobile);

            // Add EventTrigger for pointer events
            EventTrigger trigger = jumpButton.gameObject.AddComponent<EventTrigger>();

            // PointerDown
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnJumpButtonDown((PointerEventData)data); });
            trigger.triggers.Add(pointerDownEntry);

            // PointerUp
            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnJumpButtonUp((PointerEventData)data); });
            trigger.triggers.Add(pointerUpEntry);
        }
        if (flashButton != null) {
            flashButton.gameObject.SetActive(isMobile);

            // Add EventTrigger for pointer events
            EventTrigger trigger = flashButton.gameObject.AddComponent<EventTrigger>();

            // PointerDown
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnFlashButtonDown((PointerEventData)data); });
            trigger.triggers.Add(pointerDownEntry);
        }
    }

    public void OnJumpButtonDown(PointerEventData data) {
        keyDOWN = true;
        keyHOLD = true;
        keyUP = false;
    }

    public void OnJumpButtonUp(PointerEventData data) {
        keyDOWN = false;
        keyHOLD = false;
        keyUP = true;
    }

    public void OnFlashButtonDown(PointerEventData data) {
        keyFLASH = true;
    }
    
    void Update() {
        // Handles non-mobile jumping
        if (!isMobile) {
            keyDOWN = Input.GetButtonDown("Jump");
            keyHOLD = Input.GetButton("Jump");
            keyUP = Input.GetButtonUp("Jump");
            keyFLASH = Input.GetKeyDown(KeyCode.F);
        }

        // Start a new jump if conditions are met
        if (gameScript.gameSpeed != 0 && keyDOWN && (isGrounded || jumpCount < maxJumps)) {
            keyDOWN = false;
            StartJump();
            if (isGrounded) SoundEffects.Instance.PlaySound(SoundEffects.Instance.jumpStart);
            else SoundEffects.Instance.PlaySound(SoundEffects.Instance.jumpMid);
        }

        // Continue the jump while holding the button and within jump time
        if (keyHOLD && isJumping) {
            if (jumpTimeCounter > 0) {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpHoldMultiplier);
                jumpTimeCounter -= Time.deltaTime;
                fuelBar.fillAmount = jumpTimeCounter / maxJumpTime;
                animator.SetBool("isJumping", true);
            } else {
                isJumping = false;
                animator.SetBool("isJumping", false);
            }
            // THIS DOESN'T WORK AS INTENDED
            // Try to have jumpHold play until done then loop for as long as jump is held
            // SoundEffects.Instance.PlaySound(SoundEffects.Instance.jumpHold);
        }

        if (keyUP || !isJumping) {
            fuelBar.fillAmount = 0f; // resets on release
        }

        // Stop the jump if the button is released
        if (keyUP) {
            keyUP = false;
            isJumping = false;
            animator.SetBool("isJumping", false);
        }

        // Auto jump if the button is still held when landing
        if (gameScript.gameSpeed != 0 && isGrounded && keyHOLD && !isJumping) {
            StartJump();
        }

        // MODE 2
        if (gameScript.mode2 && keyFLASH && gameScript.batteryCharges > 0) {
            keyFLASH = false;
            flashAnimator.SetTrigger("flash");
            flashParticles.Play();
            gameScript.Flashed();
            SoundEffects.Instance.PlaySound(SoundEffects.Instance.flash);
        }
        else if (gameScript.mode2 && keyFLASH) {
            keyFLASH = false;
            SoundEffects.Instance.PlaySound(SoundEffects.Instance.error);
        }

        // if X position is changed somehow, slowly move back to spawn
        if (transform.position.x != spawnX) {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(spawnX, transform.position.y), 0.001f);
        }

        // limit the player's Y POS for the camera
        if (transform.position.y > maxHeight) {
            transform.position = new Vector2(transform.position.x, maxHeight);
        }

        // update the saved jump count
        availableJump = maxJumps - jumpCount;
        savedJumpUI.text = availableJump.ToString();
    }

    void FixedUpdate() {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(landingPos.position, groundDistance, groundLayer);

        // Reset jump count when grounded
        if (isGrounded) {
            jumpCount = 0;
            animator.SetBool("isJumping", false);
            if (isGroundedLock) {
                isGroundedLock = false;
                SoundEffects.Instance.PlaySound(SoundEffects.Instance.floor);
            }
        }
        else isGroundedLock = true;
    }

    private void StartJump() {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        animator.SetBool("isJumping", true);

        fuelBar.fillAmount = 1f;

        // Increment jump count on every new jump initiation
        jumpCount++;
    }
}
