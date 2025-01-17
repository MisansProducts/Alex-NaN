using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameScript : MonoBehaviour {
    // UI Text references
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] public TextMeshProUGUI flashPromptText;

    // UI References
    [SerializeField] private GameObject batteryBar;
    [SerializeField] public Image batteryBarFill;
    [SerializeField] public Image batteryBarCharges;
    [SerializeField] public Sprite[] batteryBarChargesFrames;

    // Game Objects
    [SerializeField] private GameObject player;
    [HideInInspector] private PlayerScript playerScript;
    [SerializeField] private GameObject fog;
    [HideInInspector] private FogScaleChanger fogScaleChanger;
    [SerializeField] private GameObject floorSpawner;
    [SerializeField] private GameObject outOfBounds;
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
    [SerializeField] public int powerupSpawnCooldown; // Number of cells before spawning powerups
    [SerializeField] public float batteryTime; // Time until one charge is depleted
    [SerializeField] public bool devMode = false; // Disables Game Over when killed
    [HideInInspector] public const int batteryMaxCharges = 3;
    [HideInInspector] public float battery = 1f;
    [HideInInspector] public int batteryCharges = 3;
    [HideInInspector] public float batteryCharging = 0f;
    [HideInInspector] public bool mode2 = false;
    private const float playerX = 3f;
    private const float playerY = 1.5f;
    [SerializeField] public Animator playerAnimator;

    // have player fall out of the sky and touch the ground -> solves player touching ground contact sound glitch
    // make camera not track player until player is spawned
    // nanoid attached to alexoid ??? <=> have parent object Player which has Nanoid and Alexoid
    // all permanent instantiations should happen when the game first starts

    public void Mode2(int activate) {
        switch (activate) {
            case 0: // First activated
                mode2 = true;
                batteryCharges = batteryMaxCharges;
                battery = 1f;
                batteryBarFill.fillAmount = battery;
                batteryBarCharges.sprite = batteryBarChargesFrames[2];
                batteryCharging = 0f;
                batteryBar.SetActive(true);
                flashPromptText.gameObject.SetActive(true);
                backgroundMusic.switchBGM(); // lags the game when switching to mode 2; need to make it seamless
                fogScaleChanger.enabled = true; // fog spawns
                break;
            case 1: // Deactivated
                backgroundMusic.StartGame();
                player.transform.position = new Vector3(playerX, playerY, 0);
                mode2 = false;
                batteryBar.SetActive(false);
                flashPromptText.gameObject.SetActive(false);
                break;
        }
    }

    // This is what happens when you press 'F'
    public void Flashed() {
        if (batteryCharges == batteryMaxCharges) battery = 0;
        batteryCharges -= 1;
        batteryBarCharges.sprite = batteryBarChargesFrames[batteryCharges]; // will at most be 2
        fogScaleChanger.ResetFog();
        flashPromptText.gameObject.SetActive(false); // need better code
    }

    public void UpdateScore() {
        currentScore++;
        highScore = currentScore > highScore ? currentScore : highScore;

        // Updates current and high score UI
        currentScoreText.text = currentScore.ToString();
        highScoreText.text = highScore.ToString();

        // Starts mode 2
        if (!mode2 && currentScore >= spotLightScore)
            Mode2(0);
    }

    public void GameOver() {
        if (devMode) return; // Developer mode does not end the game
        if (gameSpeed == 0) return; // Cannot activate multiple times

        float tempGameSpeed = gameSpeed;
        gameSpeed = 0;

        // Stop ongoing sounds and play death sound
        SoundEffects.Instance.StopSound();
        backgroundMusic.StopSound();
        SoundEffects.Instance.PlaySound(SoundEffects.Instance.death);

        // Plays the death animation and waits for it to finish (1.5 seconds)
        playerAnimator.Play("player_death");
        StartCoroutine(GameOver(1.5f, tempGameSpeed));
    }

    private IEnumerator GameOver(float animationTime, float tempGameSpeed) {
        yield return new WaitForSecondsRealtime(animationTime);

        PlayerPrefs.SetInt("HighScore", highScore);
        currentScore = 0;
        gameSpeed = tempGameSpeed;

        playerAnimator.Play("player_run");
        playerScript.maxJumpTime = 0.3f;
        playerScript.maxJumps = 1;
        playerScript.fuelCount = 0;
        playerScript.extraJumpCount = 0;
        fogScaleChanger.onDeath();

        // StartGame()
        // Instantiate(outOfBounds, new Vector3(7.5f, -3.5f, 0), Quaternion.identity, transform).transform.localScale = new Vector3(17, 1, 1);
        // Instantiate(outOfBounds, new Vector3(-1.5f, 4.5f, 0), Quaternion.Euler(0, 0, -90), transform).transform.localScale = new Vector3(15, 1, 1);
        // Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
        Mode2(1);
    }
    
    private void HandleBatteryBar() {
        float increase = batteryCharging == 0 ? Time.deltaTime / batteryTime : 4 * Time.deltaTime / batteryTime; // Amount to increase; quadrupled for battery pickups
        battery = Mathf.Clamp01(battery + increase); // Increases battery over time
        batteryCharging = Mathf.Clamp01(batteryCharging - Time.deltaTime); // Disables battery powerup effect after 1 second
        batteryBarFill.fillAmount = battery; // Updates battery bar UI

        // Increases battery charges after filling the battery bar
        if (battery == 1f && batteryCharges != batteryMaxCharges) {
            batteryCharges += 1;

            // Battery bar resets when filling a charge; prevents reset
            if (batteryCharges < batteryMaxCharges) {
                battery = 0f;
                batteryBarCharges.sprite = batteryBarChargesFrames[batteryCharges];
            }
        }
    }

    void Awake() {
        backgroundMusic = FindObjectOfType<BackgroundMusic>();
        fogScaleChanger = fog.GetComponent<FogScaleChanger>();
        playerScript = player.GetComponent<PlayerScript>();
    }

    void Start() {
        spikeChance = GameSetting.Instance.spikeChance;
        spikeCoolDown = GameSetting.Instance.spikeCoolDown;
        // Creates the out-of-bounds death walls
        Instantiate(outOfBounds, new Vector3(7.5f, -3.5f, 0), Quaternion.identity, transform).transform.localScale = new Vector3(17, 1, 1);
        Instantiate(outOfBounds, new Vector3(-1.5f, 4.5f, 0), Quaternion.Euler(0, 0, -90), transform).transform.localScale = new Vector3(15, 1, 1);

        // Creates the floor spawner responsible for generating the map
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);

        Mode2(1);

        if (PlayerPrefs.HasKey("HighScore")) highScore = PlayerPrefs.GetInt("HighScore");
    }

    void Update() {
        if (mode2) HandleBatteryBar();
    }
}
