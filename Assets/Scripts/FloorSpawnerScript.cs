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
    private int lastValidY = 5; // Where a floor can spawn without intersecting a platform
    private int lastValidX = 0; // Where a platform can spawn without intersecting another platform
    private const int leftEdge = -1; // 0 - 1 padding
    private const int rightEdge = 17; // 16 + 1 padding
    private int spikeCoolDownIt = 0; // iterator for hazard cooldown
    private int spikeCoolDownItPlat = 0; // hazard cooldown iterator for platforms
    private bool spawnRandomPowerup = false;
    private int powerupTimer;
    private int maxFuelCount = 3;
    private int maxExtraJumpCount = 1;

    // Function to abstract the child instaniation process
    private void CreateInstanceHelper(Transform parent, GameObject prefab, Vector3 position) {
        // Creates prefab instance
        prefab = Instantiate(prefab, position, Quaternion.identity);

        // Saves the previous scale
        Vector3 prefabScale = prefab.transform.localScale;
        float prefabX = prefabScale.x;
        float prefabY = prefabScale.y;
        float prefabZ = prefabScale.z;

        // Attaches prefab to the last floor
        prefab.transform.SetParent(parent, true);
        prefab.transform.localScale = new Vector3(prefabX / parent.localScale.x, prefabY / parent.localScale.y, prefabZ / parent.localScale.z);
    }

    // Function to generate structure
    private void GenerateStructure(Transform currFloor, int currFloorLength, float X, int Y, bool first, bool isPlatform = false) {
        int newLength = currFloorLength; // Local to hazard generation

        // =-=-=Hole Generation=-=-=
        if (!first && !isPlatform && Random.value <= gameScript.holeChance) {
            newLength = currFloorLength - 3; // 3 wide holes
            currFloor.localScale = new Vector3(newLength, 1, 1);
        }
        
        // =-=-=Main Structure Generation Loop=-=-=
        for (int i = 0; i < newLength; i++) {
            // =-=-=Cell Generation=-=-=
            CreateInstanceHelper(currFloor, cell, new Vector3(X + i, Y, 0));
            if (first) continue; // only generate cells for the first floor

            // =-=-=Spike Generation=-=-=
            if ((!isPlatform && spikeCoolDownIt == gameScript.spikeCoolDown) || (isPlatform && spikeCoolDownItPlat == gameScript.spikeCoolDown)) {
                // Randomly spawns spikes
                if (Random.value <= gameScript.spikeChance && !spawnRandomPowerup) { // !spawnRandomPowerup do not spawn hazards when powerup should spawn
                    Vector3 spikePosition = new Vector3(X + i, Y + 1, 0);
                    // Randomly chooses which type of spike to spawn
                    float randomSpike = Random.value;
                    retrySpike:
                    if (randomSpike <= gameScript.singleSpikeChance) { // Single
                        CreateInstanceHelper(currFloor, singleSpike, spikePosition);
                        if (!isPlatform) spikeCoolDownIt = 0;
                        else spikeCoolDownItPlat = 0;
                    }
                    else if (randomSpike <= gameScript.singleSpikeChance + gameScript.doubleSpikeChance) { // Double
                        if (i + 1 >= newLength) { // Fixes overhanging spikes; guaranteed singleSpike
                            randomSpike -= gameScript.doubleSpikeChance;
                            goto retrySpike;
                        }
                        CreateInstanceHelper(currFloor, doubleSpike, spikePosition);
                        if (!isPlatform) spikeCoolDownIt = -1;
                        else spikeCoolDownItPlat = -1;
                    }
                    else { // Triple
                        if (i + 2 >= newLength) { // Fixes overhanging spikes; tries for doubleSpike
                            randomSpike -= gameScript.tripleSpikeChance;
                            goto retrySpike;
                        }
                        CreateInstanceHelper(currFloor, tripleSpike, spikePosition);
                        if (!isPlatform) spikeCoolDownIt = -2;
                        else spikeCoolDownItPlat = -2;
                    }
                }
            }
            else {
                if (!isPlatform) spikeCoolDownIt++;
                else spikeCoolDownItPlat++;
            }

            // =-=-=Powerup Generation=-=-=
            if (spawnRandomPowerup) {
                if ((!isPlatform && spikeCoolDownIt < 1) || (isPlatform && spikeCoolDownItPlat < 1)) continue; // cannot spawn powerups on hazards
                Vector3 powerupPosition = new Vector3(X + i, Y + 1, 0);
                // Randomly choose a powerup to spawn
                switch (Random.Range(0, 4)) {
                    case 0: // Fuel
                        if (playerScript.fuelCount < maxFuelCount) {
                            CreateInstanceHelper(currFloor, fuelPrefab, powerupPosition);
                            break;
                        }
                        goto case 3; // skips to battery spawn
                    case 1: // ExtraJump
                        if (playerScript.extraJumpCount < maxExtraJumpCount) {
                            CreateInstanceHelper(currFloor, extraJumpPrefab, powerupPosition);
                            break;
                        }
                        goto case 3; // skips to battery spawn
                    case 2: // Shield
                        CreateInstanceHelper(currFloor, shieldPrefab, powerupPosition);
                        break;
                    case 3: // Battery
                        if (gameScript.mode2) {
                            CreateInstanceHelper(currFloor, battery, powerupPosition);
                            break;
                        }
                        goto case 2; // must be shield spawn if not mode 2
                }
                spawnRandomPowerup = false;
            }
            else HandlePowerupSpawning();
        }
    }

    // Function to handle powerup spawning
    private void HandlePowerupSpawning() {
        powerupTimer -= 1;

        if (powerupTimer == 0) {
            spawnRandomPowerup = true;
            powerupTimer = gameScript.powerupSpawnCooldown; // Reset the timer
        }
    }

    // Function to spawn floors
    private void SpawnFloor(float X = 0f, int Y = 0, bool first = true) {
        lastLength = first ? 25 : Random.Range(5, 26);
        last = Instantiate(floor, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;
        last.localScale = new Vector3(lastLength, 1, 1); // edits floor's scale
        GenerateStructure(last, lastLength, X, Y, first); // generates structure
        floorLengths.Enqueue(new FloorMeta(last, lastLength));
        // Spawns platform
        if (!first && Random.value <= 0.5f) {
            int randOffset = Random.Range(lastValidX, lastLength);
            float platformX = X + randOffset;
            int platformY = Y + Random.Range(3, 5);
            lastValidY = platformY - (platformY - Y) + 1;
            int platformLength = 5;
            lastValidX = (lastLength - 5 - randOffset < 0) ? 5 : 0;
            Transform platform = Instantiate(floor, new Vector3(platformX, platformY, 0), Quaternion.identity, transform).transform;
            platform.name = "Platform(Clone)";
            platform.localScale = new Vector3(platformLength, 1, 1);
            GenerateStructure(platform, platformLength, platformX, platformY, first, true);
            floorLengths.Enqueue(new FloorMeta(platform, platformLength));
        }
        else {
            lastValidY = 5; // Resets floor Y diversity
            lastValidX = 0; // Resets platform X restriction
        }
    }

    // Awake is called before the game starts; used to initialize object references
    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        playerScript = GameObject.FindWithTag("Player").GetComponent<PlayerScript>();
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

    }

    // Same as Update() but more consistent
    void FixedUpdate() {
        // Moves floors to the left
        foreach (Transform child in transform)
            child.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

        // Creates floors outside of the playable area
        if (last.position.x + lastLength < rightEdge)
            SpawnFloor(last.position.x + lastLength, Random.Range(0, lastValidY), false);
        
        // Deletes floors outside of the playable area
        if (firstFloor.Floor.position.x + firstFloor.Length < leftEdge) {
            Destroy(firstFloor.Floor.gameObject);
            firstFloor = floorLengths.Dequeue();
        }
    }
}
