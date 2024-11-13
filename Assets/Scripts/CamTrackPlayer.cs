using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTrackPlayer : MonoBehaviour
{
    [SerializeField] private Transform player; 
    [SerializeField] private float verticalSmoothSpeed = 0.2f;
    [SerializeField] private float upperLimit = 5f;   
    [SerializeField] private float lowerLimit = -2f;    
    [SerializeField] private float trackingThreshold = 3.5f; 

    private float originalY; 

    private void Start()
    {
        originalY = transform.position.y; // Store the camera's original Y position
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float targetY;

        if (player.position.y > trackingThreshold){ // If the player is above the tracking threshold
            targetY = player.position.y; 
        }
        else{ // If the player is below the tracking threshold
            targetY = originalY; // Set the target Y to the camera's original Y position
        }

        float smoothY = Mathf.Lerp(transform.position.y, targetY, verticalSmoothSpeed); // Smoothly move the camera's Y position towards the target Y position

        float clampedY = Mathf.Clamp(smoothY, lowerLimit, upperLimit); // Clamp the Y position between the upper and lower limits

        Vector3 newPosition = new Vector3(transform.position.x, clampedY, transform.position.z); // Create a new position with the clamped Y position

        transform.position = newPosition; // Set the camera's position to the new position
    }
}
