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
    private float jumpTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        // get input for jumping while player is on the ground 
        if(isOnGround && Input.GetButton("Jump")){
            isJumping = true;
            rb.velocity = Vector2.up * jumpForce;
        }

        // if the player is in the air and player is still holding the jump button
        if(isJumping){
            if (jumpTimer < jumpTime){ // give it another push
                rb.velocity = Vector2.up * jumpForce;
                jumpTimer += Time.deltaTime;
            } 
            else { // drop his ass
                isJumping = false;
            }
            
        }

        if(Input.GetButtonUp("Jump")){
            isJumping = false;
            jumpTimer = 0;
        }
    }

    void FixedUpdate() {
        isOnGround = Physics2D.OverlapCircle(landingPos.position, groundDistance, floorLayer);

        // Reset jump timer when landing
        if (isOnGround) jumpTimer = 0;
    }

}
