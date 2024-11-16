using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameScript : MonoBehaviour {
    // UI Text references
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] public TextMeshProUGUI flashPromptText;

    // Game Objects
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject floorSpawner;
    [SerializeField] private GameObject platformSpawner;
    [SerializeField] private GameObject batteryBar;
    [SerializeField] public Image batteryBarFill;
    [SerializeField] public Light2D spotLight;

    // Game Variables
    [SerializeField] public int currentScore;
    [SerializeField] private int highScore;
    [SerializeField] public float spotLightScore; // Score in which mode 2 is activated
    [SerializeField] public float gameSpeed;
    [SerializeField] public float spikeChance;
    [SerializeField] public bool devMode = false; // Disables Game Over when killed
    [HideInInspector] public float spotLightTime = 0f; // Time until spotlight is deactivated
    [HideInInspector] public float battery = 1f;
    [HideInInspector] public const float batteryTime = 33f; // Time until entire battery is depleted
    [HideInInspector] public bool mode2 = false;
    private float previousGameSpeed;
    private const float playerX = 3f;
    private const float playerY = 1.5f;

    private void StartGame() {
        Mode2(2);
        //Instantiate(player, new Vector3(playerX, playerY, 0), Quaternion.identity, transform);
        player.transform.position = new Vector3(playerX, playerY, 0);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
    }

    public void Mode2(int activate) {
        switch (activate) {
            case 0: // First activated
                gameSpeed = 0f;
                mode2 = true;
                battery = 1f;
                batteryBarFill.fillAmount = battery;
                batteryBar.SetActive(true);
                flashPromptText.gameObject.SetActive(true);
                break;
            case 1: // Fully activated
                gameSpeed = previousGameSpeed;
                flashPromptText.gameObject.SetActive(false);
                break;
            case 2: // Deactivated
                mode2 = false;
                batteryBar.SetActive(false);
                flashPromptText.gameObject.SetActive(false);
                break;
        }
    }

    public void UpdateScore() {
        currentScore++;
        highScore = currentScore > highScore ? currentScore : highScore;
        currentScoreText.text = "Score: " + currentScore.ToString();
        highScoreText.text = "Hi-Score: " + highScore.ToString();
        if (!mode2) {
            spotLight.intensity = Mathf.Lerp(1f, 0f, currentScore / spotLightScore);
            if (currentScore >= spotLightScore) Mode2(0);
        }
    }

    public void GameOver() {
        if(devMode == false) {
            gameSpeed = 0f;
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
            gameSpeed = previousGameSpeed;
            currentScore = 0;
            spotLight.intensity = 1f;
            spotLightTime = 0f;
            StartGame();
        }
    }

    void Start() {
        previousGameSpeed = gameSpeed;
        StartGame();
    }

    void Update() {
        if (gameSpeed != 0 && mode2) {
            spotLightTime = Mathf.Clamp01(spotLightTime + Time.deltaTime / 10f);
            spotLight.intensity = Mathf.SmoothStep(1f, 0f, spotLightTime);
            battery = Mathf.Clamp01(battery + Time.deltaTime / batteryTime);
            batteryBarFill.fillAmount = battery;
        }
    }
}
