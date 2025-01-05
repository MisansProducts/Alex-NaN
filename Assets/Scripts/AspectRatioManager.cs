using UnityEngine;

public class AspectRatioManager : MonoBehaviour
{
    public float targetAspect = 16f / 9f;

    private void Start()
    {
        UpdateAspectRatio();
    }

    private void UpdateAspectRatio()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f) 
        {
            Camera.main.rect = new Rect((1.0f - scaleHeight) / 2.0f, 0.0f, scaleHeight, 1.0f);
        }
        else // Add black bars on the top and bottom
        {
            float scaleWidth = 1.0f / scaleHeight;
            Camera.main.rect = new Rect(0.0f, (1.0f - scaleWidth) / 2.0f, 1.0f, scaleWidth);
        }
    }

    private void Update()
    {
        UpdateAspectRatio();
    }
}
