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
    [HideInInspector] public float battery = 1f;
    [HideInInspector] public const float batteryTime = 33f; // Time until entire battery is depleted
    [HideInInspector] public const float batteryDrainAmount = 1f / 3f;
    [HideInInspector] public bool mode2 = false;
    [HideInInspector] private FogScaleChanger fogScaleChanger;
    private const float playerX = 3f;
    private const float playerY = 1.5f;

    private void StartGame() {
        backgroundMusic.StartGame();
        Mode2(1);
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
                mode2 = true;
                battery = 1f;
                batteryBarFill.fillAmount = battery;
                batteryBar.SetActive(true);
                flashPromptText.gameObject.SetActive(true);
                // backgroundMusic.switchBGM(); // lags the game when switching to mode 2; need to make it seamless
                fogScaleChanger.enabled = true; // fog spawns
                break;
            case 1: // Deactivated
                mode2 = false;
                batteryBar.SetActive(false);
                flashPromptText.gameObject.SetActive(false);
                break;
        }
    }

    // This is what happens when you press 'F'
    public void Flashed() {
        battery -= batteryDrainAmount;
        fogScaleChanger.ResetFog();
        flashPromptText.gameObject.SetActive(false); // need better code
    }

    public void UpdateScore() {
        currentScore++;
        highScore = currentScore > highScore ? currentScore : highScore;
        currentScoreText.text = "Score: " + currentScore.ToString();
        highScoreText.text = "Hi-Score: " + highScore.ToString();
        if (!mode2 && currentScore >= spotLightScore) Mode2(0);
    }

    public void ResetPowerUps() {
        player.GetComponent<PlayerScript>().maxJumpTime = 0.3f;
        player.GetComponent<PlayerScript>().maxJumps = 1;
        player.GetComponent<PlayerScript>().fuelCount = 0;
        player.GetComponent<PlayerScript>().extraJumpCount = 0;
        fogScaleChanger.onDeath(); //reset fog here too
    } 

    public void GameOver() {
        if(devMode == false) {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            currentScore = 0;
            ResetPowerUps();
            StartGame();
        }
    }

    void Awake() {
        backgroundMusic = FindObjectOfType<BackgroundMusic>();
        fogScaleChanger = fog.GetComponent<FogScaleChanger>();
    }

    void Start() {
        StartGame();
    }

    void Update() {
        if (mode2) {
            battery = Mathf.Clamp01(battery + Time.deltaTime / batteryTime);
            batteryBarFill.fillAmount = battery;
        }
    }
}
