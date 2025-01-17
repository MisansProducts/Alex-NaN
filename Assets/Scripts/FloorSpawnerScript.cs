using System.Collections.Generic;
using UnityEngine;

public class FloorSpawnerScript : MonoBehaviour {
    // Objects
    private GameScript gameScript;
    private PlayerScript playerScript;
    private Transform last; // Last floor spawned
    private enum Inst {floor = 1, cell = 2, singleSpike = 3, doubleSpike = 4, tripleSpike = 5, battery = 6, fuelPrefab = 7, shieldPrefab = 8, extraJumpPrefab = 9};
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
    private List<GameObject> active_floor = new List<GameObject>();
    private List<GameObject> active_cell = new List<GameObject>();
    private List<GameObject> active_singleSpike = new List<GameObject>();
    private List<GameObject> active_doubleSpike = new List<GameObject>();
    private List<GameObject> active_tripleSpike = new List<GameObject>();
    private List<GameObject> active_battery = new List<GameObject>();
    private List<GameObject> active_fuelPrefab = new List<GameObject>();
    private List<GameObject> active_shieldPrefab = new List<GameObject>();
    private List<GameObject> active_extraJumpPrefab = new List<GameObject>();
    private List<GameObject> reserve_floor = new List<GameObject>();
    private List<GameObject> reserve_cell = new List<GameObject>();
    private List<GameObject> reserve_singleSpike = new List<GameObject>();
    private List<GameObject> reserve_doubleSpike = new List<GameObject>();
    private List<GameObject> reserve_tripleSpike = new List<GameObject>();
    private List<GameObject> reserve_battery = new List<GameObject>();
    private List<GameObject> reserve_fuelPrefab = new List<GameObject>();
    private List<GameObject> reserve_shieldPrefab = new List<GameObject>();
    private List<GameObject> reserve_extraJumpPrefab = new List<GameObject>();
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
    private void CreateInstanceHelper(Transform parent, Inst type, Vector3 position) {
        // Creates prefab instance
        Transform prefab = null;
        switch (type) {
            case Inst.cell: {
                active_cell.Add(reserve_cell[0]);
                prefab = reserve_cell[0].transform;
                reserve_cell.RemoveAt(0);
                break;
            }
            case Inst.singleSpike: {
                active_singleSpike.Add(reserve_singleSpike[0]);
                prefab = reserve_singleSpike[0].transform;
                reserve_singleSpike.RemoveAt(0);
                break;
            }
            case Inst.doubleSpike: {
                active_doubleSpike.Add(reserve_doubleSpike[0]);
                prefab = reserve_doubleSpike[0].transform;
                reserve_doubleSpike.RemoveAt(0);
                break;
            }
            case Inst.tripleSpike: {
                active_tripleSpike.Add(reserve_tripleSpike[0]);
                prefab = reserve_tripleSpike[0].transform;
                reserve_tripleSpike.RemoveAt(0);
                break;
            }
            case Inst.battery: {
                active_battery.Add(reserve_battery[0]);
                prefab = reserve_battery[0].transform;
                reserve_battery.RemoveAt(0);
                break;
            }
            case Inst.shieldPrefab: {
                active_shieldPrefab.Add(reserve_shieldPrefab[0]);
                prefab = reserve_shieldPrefab[0].transform;
                reserve_shieldPrefab.RemoveAt(0);
                break;
            }
            case Inst.fuelPrefab: {
                active_fuelPrefab.Add(reserve_fuelPrefab[0]);
                prefab = reserve_fuelPrefab[0].transform;
                reserve_fuelPrefab.RemoveAt(0);
                break;
            }
            case Inst.extraJumpPrefab: {
                active_extraJumpPrefab.Add(reserve_extraJumpPrefab[0]);
                prefab = reserve_extraJumpPrefab[0].transform;
                reserve_extraJumpPrefab.RemoveAt(0);
                break;
            }
        }
        prefab.transform.position = position;

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
            CreateInstanceHelper(currFloor, Inst.cell, new Vector3(X + i, Y, 0));
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
                        CreateInstanceHelper(currFloor, Inst.singleSpike, spikePosition);
                        if (!isPlatform) spikeCoolDownIt = 0;
                        else spikeCoolDownItPlat = 0;
                    }
                    else if (randomSpike <= gameScript.singleSpikeChance + gameScript.doubleSpikeChance) { // Double
                        if (i + 1 >= newLength) { // Fixes overhanging spikes; guaranteed singleSpike
                            randomSpike -= gameScript.doubleSpikeChance;
                            goto retrySpike;
                        }
                        CreateInstanceHelper(currFloor, Inst.doubleSpike, spikePosition);
                        if (!isPlatform) spikeCoolDownIt = -1;
                        else spikeCoolDownItPlat = -1;
                    }
                    else { // Triple
                        if (i + 2 >= newLength) { // Fixes overhanging spikes; tries for doubleSpike
                            randomSpike -= gameScript.tripleSpikeChance;
                            goto retrySpike;
                        }
                        CreateInstanceHelper(currFloor, Inst.tripleSpike, spikePosition);
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
                            CreateInstanceHelper(currFloor, Inst.fuelPrefab, powerupPosition);
                            break;
                        }
                        goto case 3; // skips to battery spawn
                    case 1: // ExtraJump
                        if (playerScript.extraJumpCount < maxExtraJumpCount) {
                            CreateInstanceHelper(currFloor, Inst.extraJumpPrefab, powerupPosition);
                            break;
                        }
                        goto case 3; // skips to battery spawn
                    case 2: // Shield
                        CreateInstanceHelper(currFloor, Inst.shieldPrefab, powerupPosition);
                        break;
                    case 3: // Battery
                        if (gameScript.mode2) {
                            CreateInstanceHelper(currFloor, Inst.battery, powerupPosition);
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
        active_floor.Add(reserve_floor[0]);
        last = reserve_floor[0].transform;
        reserve_floor.RemoveAt(0);
        last.position = new Vector3(X, Y, 0);
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

        // Instantiates reserve
        for (int i = 0; i < 10; i++) reserve_floor.Add(Instantiate(floor, new Vector3(100, 100, 0), Quaternion.identity));
        for (int i = 0; i < 50; i++) reserve_cell.Add(Instantiate(cell, new Vector3(100, 100, 0), Quaternion.identity));
        for (int i = 0; i < 20; i++) reserve_singleSpike.Add(Instantiate(singleSpike, new Vector3(100, 100, 0), Quaternion.identity));
        for (int i = 0; i < 20; i++) reserve_doubleSpike.Add(Instantiate(doubleSpike, new Vector3(100, 100, 0), Quaternion.identity));
        for (int i = 0; i < 20; i++) reserve_tripleSpike.Add(Instantiate(tripleSpike, new Vector3(100, 100, 0), Quaternion.identity));
        reserve_battery.Add(Instantiate(battery, new Vector3(100, 100, 0), Quaternion.identity));
        reserve_shieldPrefab.Add(Instantiate(shieldPrefab, new Vector3(100, 100, 0), Quaternion.identity));
        reserve_fuelPrefab.Add(Instantiate(fuelPrefab, new Vector3(100, 100, 0), Quaternion.identity));
        reserve_extraJumpPrefab.Add(Instantiate(extraJumpPrefab, new Vector3(100, 100, 0), Quaternion.identity));

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
        foreach (GameObject child in active_floor) // FIX THIS
            child.transform.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

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
