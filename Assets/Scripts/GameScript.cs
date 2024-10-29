using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GameScript : MonoBehaviour {
    public GameObject player;
    public GameObject floorSpawner;
    public Light2D spotLight;
    public GameObject batteryBar;
    public GameObject batteryBackground;
    public GameObject batteryBorder;
    public GameObject batteryPromptText;
    [SerializeField] public float gameSpeed = 5f;
    [SerializeField] public float spikeChance = 0.2f;
    public int currentScore = 0;
    public int highScore = 0;
    private const float spotLightScore = 200f; // Score in which mode 2 is activated
    public float spotLightTime = 0f;
    public float battery = 1f;
    public const float batteryTime = 33f;
    [SerializeField] public bool mode2 = false;

    // UI Text references
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;

    public void UpdateScore() {
        currentScore++;
        highScore = currentScore > highScore ? currentScore : highScore;
        currentScoreText.text = "Score: " + currentScore.ToString();
        highScoreText.text = "Hi-Score: " + highScore.ToString();
        if (!mode2) {
            spotLight.intensity = Mathf.Lerp(1f, 0f, currentScore / spotLightScore);
            if (currentScore >= spotLightScore) {
                gameSpeed = 0f;
                mode2 = true;
                batteryBackground.SetActive(true);
                batteryBar.SetActive(true);
                batteryBorder.SetActive(true);
                batteryPromptText.SetActive(true);
            }
        }
    }

    public void GameOver() {
        gameSpeed = 0f;
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameSpeed = 5f;
        currentScore = 0;
        spotLight.intensity = 1f;
        spotLightTime = 0f;
        battery = 1f;
        mode2 = false;

        batteryBackground.SetActive(false);
        batteryBar.SetActive(false);
        batteryBorder.SetActive(false);
        batteryPromptText.SetActive(false);

        Instantiate(player, new Vector3(1.5f, 1.5f, 0), Quaternion.identity, transform);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
    }

    // Start is called before the first frame update
    void Start() {
        spotLight = GameObject.FindGameObjectWithTag("Light").GetComponent<Light2D>();
        batteryBar = GameObject.FindGameObjectWithTag("Battery");
        batteryBackground = GameObject.FindGameObjectWithTag("BatteryBG");
        batteryBorder = GameObject.FindGameObjectWithTag("BatteryBorder");
        batteryPromptText = GameObject.FindGameObjectWithTag("BatteryPrompt");
        Instantiate(player, new Vector3(1.5f, 1.5f, 0), Quaternion.identity, transform);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
        
        batteryBackground.SetActive(false);
        batteryBar.SetActive(false);
        batteryBorder.SetActive(false);
        batteryPromptText.SetActive(false);
    }

    void Update() {
        if (gameSpeed != 0 && mode2) {
            spotLightTime = Mathf.Clamp01(spotLightTime + Time.deltaTime / 10f);
            spotLight.intensity = Mathf.SmoothStep(1f, 0f, spotLightTime);
            battery = Mathf.Clamp01(battery + Time.deltaTime / batteryTime);
            batteryBar.GetComponent<Image>().fillAmount = battery;
        }
    }
}
