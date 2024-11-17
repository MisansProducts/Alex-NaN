using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parallaxBackground : MonoBehaviour
{
    public GameObject otherBackground;  // Reference to the other background object
    private float length;               // Width of the background sprite
    [SerializeField] public float parallaxEffect;
    [SerializeField] public float gameSpeed;

    void Start()
    {
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Move background left continuously at the parallax speed
        float moveAmount = gameSpeed * parallaxEffect * Time.deltaTime;
        transform.Translate(Vector3.left * moveAmount);

        // Check if this background has moved off-screen
        if (transform.position.x < -length)
        {
            // Move this background to the right of the other background
            transform.position = new Vector3(otherBackground.transform.position.x + length, transform.position.y, transform.position.z);
        }
    }
}
