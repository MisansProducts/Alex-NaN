using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogScaleChanger : MonoBehaviour
{
    public float scaleSpeed = 0.5f;
    public float minScale = 1.0f;  
    public float maxScale = 3.0f;  

    public float targetScale; // The scale value we're moving towards
    public Vector3 originalScale;

    void Start()
    {
        enabled = false;
        // Store the original scale
        originalScale = transform.localScale;
        targetScale = maxScale;
    }

    void Update()
    {
        // change the X scale over time
        float newScaleX = Mathf.MoveTowards(transform.localScale.x, targetScale, scaleSpeed * Time.deltaTime);

        // scale it
        transform.localScale = new Vector3(newScaleX, originalScale.y, originalScale.z);
        
        // stop scaling when we reach the target scale
        if (transform.localScale.x == targetScale)
        {
            // stop scaling  
            enabled = false;
        }
    }

    public void ResetFog()
    {
        // reset the scale
        enabled = false;
        transform.localScale = originalScale;
        targetScale = maxScale;
        enabled = true;
    }
    public void onDeath()
    {
        // reset the scale
        enabled = false;
        transform.localScale = originalScale;
        targetScale = maxScale;
    }
}
