using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform landingPos;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private float jumpTime = 0.2f;
    private bool isOnGround = false;
    private bool isJumping = false;
    private int jumpCount = 0;
    [SerializeField] private int maxJumps = 2; // Allow extra jumps since your spike spawns are complete random 
    //and sometimes it is IMPOSSIBLE to jump over them
    private float jumpTimer = 0f;
    private GameScript gameScript;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Spike"))
        {
            gameScript.GameOver();
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameScript = FindObjectOfType<GameScript>();
    }

    void Update()
    {
        // Jumping logic
        if (Input.GetButtonDown("Jump") && (isOnGround || jumpCount < maxJumps))
        {
            isJumping = true;
            rb.velocity = Vector2.up * jumpForce;
            jumpCount++; // Increment jump count on each jump
            jumpTimer = 0;
        }

        // Continuous jump boost while holding jump button
        if (isJumping && Input.GetButton("Jump"))
        {
            if (jumpTimer < jumpTime)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpTimer += Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // Reset jump variables when releasing jump button
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
            jumpTimer = 0;
        }
    }

    void FixedUpdate()
    {
        isOnGround = Physics2D.OverlapCircle(landingPos.position, groundDistance, floorLayer);

        // Reset jump count and timer when on the ground
        if (isOnGround)
        {
            jumpCount = 0;
            jumpTimer = 0;
        }
    }
}
