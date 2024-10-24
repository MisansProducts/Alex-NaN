using UnityEngine;

public class PlayerMovement : MonoBehaviour
{   
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private Transform landingPos;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private float jumpTime = 0.5f;
    private bool isOnGround = false;
    private bool isJumping = false;
    private float jumpTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        isOnGround = Physics2D.OverlapCircle(landingPos.position, groundDistance, floorLayer);

        // get input for jumping while player is on the ground 
        if(isOnGround && Input.GetButtonDown("Jump")){
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
}
