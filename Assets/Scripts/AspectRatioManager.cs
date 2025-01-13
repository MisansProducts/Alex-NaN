using UnityEngine;

public class AspectRatioManager : MonoBehaviour {
    public Camera cam;
    public float targetAspect = 16.0f / 9.0f;
    private int lastWidth;
    private int lastHeight;
    private bool isMobile;
    
    
    void Start() {
        isMobile = Application.isMobilePlatform;
        cam = GetComponent<Camera>();
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        if (isMobile) Screen.SetResolution(1920, 1080, true); // mobile cannot be windowed
        if (isMobile || !Screen.fullScreen) ApplyLetterbox(); // mobile should apply black bars, PC only if windowed
    }

    void Update() {
        if (Screen.width != lastWidth || Screen.height != lastHeight) {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            ApplyLetterbox();
        }
    }

    void ApplyLetterbox() {
        float currentAspect = (float)Screen.width / Screen.height;
        float bgWidth = 1.0f;
        float bgHeight = 1.0f;

        if (currentAspect > targetAspect) {
            bgWidth = targetAspect / currentAspect;
            bgHeight = 1.0f;
        }
        else {
            bgWidth = 1.0f;
            bgHeight = currentAspect / targetAspect;
        }

        cam.rect = new Rect(
            (1.0f - bgWidth) / 2.0f,
            (1.0f - bgHeight) / 2.0f,
            bgWidth,
            bgHeight
        );
    }
}
