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
    [SerializeField] private GameObject fog;
    [SerializeField] private GameObject floorSpawner;
    [SerializeField] private GameObject outOfBounds;
    [SerializeField] private GameObject batteryBar;
    [SerializeField] public Image batteryBarFill;
    [SerializeField] public Light2D spotLight;
    [SerializeField] private BackgroundMusic backgroundMusic;

    // Game Variables
    [SerializeField] public int currentScore;
    [SerializeField] private int highScore;
    [SerializeField] public float spotLightScore; // Score in which mode 2 is activated
    [SerializeField] public float gameSpeed;
    [SerializeField] public float spikeChance;
    [SerializeField] public int spikeCoolDown = 2; // At least 2 safe spaces before generating spikes
    [SerializeField] public float singleSpikeChance;
    [SerializeField] public float doubleSpikeChance;
    [SerializeField] public float tripleSpikeChance;
    [SerializeField] public float holeChance;
    [SerializeField] public float powerupSpawnCooldown = 20f; // Cooldown time for spawning powerups
    [SerializeField] public bool devMode = false; // Disables Game Over when killed
    [HideInInspector] public float spotLightTime = 0f; // Time until spotlight is deactivated
    [HideInInspector] public float battery = 1f;
    [HideInInspector] public const float batteryTime = 33f; // Time until entire battery is depleted
    [HideInInspector] public bool mode2 = false;
    private float previousGameSpeed;
    private const float playerX = 3f;
    private const float playerY = 1.5f;

    private void StartGame() {
        backgroundMusic.StartGame();
        Mode2(2);
        //Instantiate(player, new Vector3(playerX, playerY, 0), Quaternion.identity, transform);
        player.transform.position = new Vector3(playerX, playerY, 0);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);

        // Creates the out-of-bounds death walls
        GameObject oob = Instantiate(outOfBounds, new Vector3(7.5f, -3.5f, 0), Quaternion.identity, transform);
        oob.transform.localScale = new Vector3(17, 1, 1);
        oob = Instantiate(outOfBounds, new Vector3(-1.5f, 4.5f, 0), Quaternion.Euler(0, 0, -90), transform);
        oob.transform.localScale = new Vector3(15, 1, 1);
    }

    public void Mode2(int activate) {
        switch (activate) {
            case 0: // First activated
                //gameSpeed = 0f;
                mode2 = true;
                battery = 1f;
                batteryBarFill.fillAmount = battery;
                batteryBar.SetActive(true);
                flashPromptText.gameObject.SetActive(true);
                backgroundMusic.switchBGM();
                break;
            case 1: // Fully activated
                gameSpeed = previousGameSpeed;
                flashPromptText.gameObject.SetActive(false);
                fog.GetComponent<FogScaleChanger>().enabled = true; // fog spawns
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

    public void ResetPowerUps() {
        player.GetComponent<PlayerScript>().maxJumpTime = 0.3f;
        player.GetComponent<PlayerScript>().maxJumps = 1;
        player.GetComponent<PlayerScript>().fuelCount = 0;
        player.GetComponent<PlayerScript>().extraJumpCount = 0;
        fog.GetComponent<FogScaleChanger>().ResetFog(); //reset fog here too
    } 

    public void GameOver() {
        if(devMode == false) {
            gameSpeed = 0f;
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            gameSpeed = previousGameSpeed;
            currentScore = 0;
            spotLight.intensity = 1f; //change here
            spotLightTime = 0f;
            ResetPowerUps();
            StartGame();
        }
    }

    void Awake() {
        backgroundMusic = FindObjectOfType<BackgroundMusic>();
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
