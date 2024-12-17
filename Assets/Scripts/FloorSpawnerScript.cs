using System.Collections.Generic;
using UnityEngine;

public class FloorSpawnerScript : MonoBehaviour {

    // Objects
    private GameScript gameScript;
    private PlayerScript playerScript;
    private Transform last; // Last floor spawned
    [SerializeField] private GameObject floor; // Floor prefab
    [SerializeField] private GameObject cell; // 1x1 cell prefab
    [SerializeField] private GameObject singleSpike, doubleSpike, tripleSpike; // Hazards
    [SerializeField] private GameObject battery;
    // Add new powerup prefabs
    [SerializeField] private GameObject fuelPrefab; // Adds maxjumptime 
    [SerializeField] private GameObject shieldPrefab; // Adds invincibility for one hit
    [SerializeField] private GameObject extraJumpPrefab; // Adds an extra jump
    
    // Variables
    private struct FloorMeta {
        public Transform Floor;
        public int Length;

        public FloorMeta(Transform floor, int length) {
            Floor = floor;
            Length = length;
        }
        
    }
    private Queue<FloorMeta> floorLengths = new Queue<FloorMeta>();
    private FloorMeta firstFloor;
    private int lastLength = 25;
    private const int leftEdge = -1; // 0 - 1 padding
    private const int rightEdge = 17; // 16 + 1 padding
    private int floorNumber = 1; // for debugging
    private int spikeCoolDownIt = 0; // iterator for hazard cooldown
    private bool spawnRandomPowerup = false;
    private float powerupTimer;
    private int maxFuelCount = 3;
    private int maxExtraJumpCount = 1;

    // Function to generate structure
    private void GenerateStructure(float X, int Y, bool first) {
        int newLastLength = lastLength; // Local to hazard generation
        if (first) goto cellGeneration; // skips hole gen on first floor
        // =-=-=Hole Generation=-=-=
        if (Random.value <= gameScript.holeChance) {
            newLastLength = lastLength - 3; // 3 wide holes
            last.localScale = new Vector3(newLastLength, 1, 1);
        }
        // =-=-=Main Structure Generation Loop=-=-=
        cellGeneration:
        for (int i = 0; i < newLastLength; i++) {
            // =-=-=Cell Generation=-=-=
            Vector3 cellPosition = new Vector3(X + i, Y, 0);
            GameObject cellPrefab = Instantiate(cell, cellPosition, Quaternion.identity);
            cellPrefab.transform.SetParent(last, true);
            cellPrefab.transform.localScale = new Vector3(1f / last.localScale.x, 1f / last.localScale.y, 1f / last.localScale.z);
            if (first) continue; // skips hazard gen on first floor
            // =-=-=Spike Generation=-=-=
            if (spikeCoolDownIt == gameScript.spikeCoolDown) {
                // Randomly spawns spikes
                if (Random.value <= gameScript.spikeChance && !spawnRandomPowerup) { // !spawnRandomPowerup do not spawn hazards when powerup should spawn
                    Vector3 spikePosition = new Vector3(X + i, Y + 1, 0);
                    GameObject spikePrefab = null;
                    // Randomly chooses which type of spike to spawn
                    float randomSpike = Random.value;
                    retrySpike:
                    if (randomSpike <= gameScript.singleSpikeChance) { // Single
                        spikePrefab = Instantiate(singleSpike, spikePosition, Quaternion.identity);
                        spikeCoolDownIt = 0;
                    }
                    else if (randomSpike <= gameScript.singleSpikeChance + gameScript.doubleSpikeChance) { // Double
                        if (i + 1 >= newLastLength) { // Fixes overhanging spikes; guaranteed singleSpike
                            randomSpike -= gameScript.doubleSpikeChance;
                            goto retrySpike;
                        }
                        spikePrefab = Instantiate(doubleSpike, spikePosition, Quaternion.identity);
                        spikeCoolDownIt = -1;
                    }
                    else if (randomSpike > gameScript.singleSpikeChance + gameScript.doubleSpikeChance) { // Triple
                        if (i + 2 >= newLastLength) { // Fixes overhanging spikes; tries for doubleSpike
                            randomSpike -= gameScript.tripleSpikeChance;
                            goto retrySpike;
                        }
                        spikePrefab = Instantiate(tripleSpike, spikePosition, Quaternion.identity);
                        spikeCoolDownIt = -2;
                    }
                    if (spikePrefab != null) {
                        spikePrefab.transform.SetParent(last, true);
                        spikePrefab.transform.localScale = new Vector3(1f / last.localScale.x, 1f / last.localScale.y, 1f / last.localScale.z);
                    }
                }
            }
            else spikeCoolDownIt++;
            // =-=-=Powerup Generation=-=-=
            if (spawnRandomPowerup) {
                if (spikeCoolDownIt < 0) continue; // cannot spawn powerups on hazards
                Vector3 powerupPosition = new Vector3(X + i, Y + 1, 0);
                GameObject powerupInstance = null;
                // Randomly choose a powerup to spawn
                switch (Random.Range(0, 3)) {
                    case 0: // Fuel
                        if (playerScript.fuelCount < maxFuelCount) {
                            powerupInstance = Instantiate(fuelPrefab, powerupPosition, Quaternion.identity);
                        }
                        break;
                    case 1: // ExtraJump
                        if (playerScript.extraJumpCount < maxExtraJumpCount) {
                            powerupInstance = Instantiate(extraJumpPrefab, powerupPosition, Quaternion.identity);
                        }
                        break;
                    case 2: // Shield
                        powerupInstance = Instantiate(shieldPrefab, powerupPosition, Quaternion.identity);
                        break;
                }
                if (powerupInstance != null) {
                    powerupInstance.transform.SetParent(last, true);
                    powerupInstance.transform.localScale = new Vector3(0.5f / last.localScale.x, 0.5f / last.localScale.y, 0.5f / last.localScale.z);
                    spawnRandomPowerup = false;
                }
            }
        }
    }

    // Function to handle powerup spawning
    private void HandlePowerupSpawning() {
        powerupTimer -= Time.deltaTime;

        if (powerupTimer <= 0) {
            spawnRandomPowerup = true;
            powerupTimer = gameScript.powerupSpawnCooldown; // Reset the timer
        }
    }

    // Function to spawn floors
    private void SpawnFloor(float X = 0f, int Y = 0, bool first = true) {
        print($"Floor number: {floorNumber}"); // for debugging
        floorNumber++;
        lastLength = first ? 25 : Random.Range(5, 25);
        last = Instantiate(floor, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;
        last.localScale = new Vector3(lastLength, 1, 1); // edits floor's scale
        last.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // random color for testing
        GenerateStructure(X, Y, first); // generates structure
        floorLengths.Enqueue(new FloorMeta(last, lastLength));
    }

    // Awake is called before the game starts; used to initialize object references
    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null) {
            playerScript = playerObject.GetComponent<PlayerScript>();
        }
    }

    // Start is called before the first frame update
    void Start() {
        // Normalizes spike chance (will error on sum == 0)
        float spikeSum = gameScript.singleSpikeChance + gameScript.doubleSpikeChance + gameScript.tripleSpikeChance;
        gameScript.singleSpikeChance /= spikeSum;
        gameScript.doubleSpikeChance /= spikeSum;
        gameScript.tripleSpikeChance /= spikeSum;
        
        // Initialize Variables
        spikeCoolDownIt = gameScript.spikeCoolDown;

        // Spawns the first floor
        SpawnFloor();
        firstFloor = floorLengths.Dequeue();

        // Starts the powerup timer
        powerupTimer = gameScript.powerupSpawnCooldown;
    }

    // Update is called once per frame
    void Update() {
        HandlePowerupSpawning();
    }

    // Same as Update() but more consistent
    void FixedUpdate() {
        // Moves floors to the left
        foreach (Transform child in transform)
            child.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

        // Creates floors outside of the playable area
        if (last.position.x + lastLength < rightEdge)
            SpawnFloor(last.position.x + lastLength, Random.Range(1, 5), false);
        
        // Deletes floors outside of the playable area
        if (firstFloor.Floor.position.x + firstFloor.Length < leftEdge) {
            Destroy(firstFloor.Floor.gameObject);
            firstFloor = floorLengths.Dequeue();
        }
    }
}
