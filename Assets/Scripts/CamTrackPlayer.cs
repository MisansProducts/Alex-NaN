using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTrackPlayer : MonoBehaviour
{
    [SerializeField] private Transform player; 
    [SerializeField] private float verticalSmoothSpeed;
    [SerializeField] private float upperLimit;   
    [SerializeField] private float lowerLimit;    
    [SerializeField] private float trackingThreshold; 

    private float originalY; 

    private void Start() {
        originalY = transform.position.y; // Store the camera's original Y position
    }

    private void LateUpdate() {
        if (player == null) return;

        // Sets the target Y for tracking
        float targetY;
        if (player.position.y > trackingThreshold) targetY = player.position.y; // If the player is above the tracking threshold
        else targetY = originalY; // Below the tracking threshold; set the target Y to the camera's original Y position

        float clampedY = Mathf.Clamp(targetY, lowerLimit, upperLimit); // Clamp the Y position between the upper and lower limits
        Vector3 newPosition = new Vector3(transform.position.x, clampedY, transform.position.z); // Create a new position with the clamped Y position
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * verticalSmoothSpeed);
    }
}
