using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebFogScaler : MonoBehaviour
{
    [SerializeField] public GameObject webFog;
    public float scaleSpeed = 0.5f;
    public float minScale = 1.0f;  
    public float maxScale = 3.0f;  

    public float targetScale; // The scale value we're moving towards
    public Vector3 originalScale;

    void Start(){
        enabled = false;
        originalScale = webFog.transform.localScale;
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

    public void ResetWebFog(){
        //reset the scale of the webFog object
        webFog.transform.localScale = new Vector3(3f, 1.43f, 0f);
    }

    public void onDeath(){
        enabled = false;
        transform.localScale = originalScale;
        targetScale = maxScale;
    }
}
