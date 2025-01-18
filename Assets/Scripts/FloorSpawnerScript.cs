using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FloorSpawnerScript : MonoBehaviour {
    // Objects
    private GameScript gameScript;
    private PlayerScript playerScript;
    private Transform last; // Last floor spawned
    private enum Inst {floor = 0, cell = 1, singleSpike = 2, doubleSpike = 3, tripleSpike = 4, battery = 5, fuelPrefab = 6, shieldPrefab = 7, extraJumpPrefab = 8};
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
    private struct ChildMeta {
        public Transform Parent;
        public GameObject Object;
        public bool IsCell;
        
        public ChildMeta(Transform parent, GameObject obj, bool isCell = false) {
            Parent = parent;
            Object = obj;
            IsCell = isCell;
        }
    }
    private Queue<FloorMeta> floorLengths = new Queue<FloorMeta>();
    private List<List<ChildMeta>> active = new List<List<ChildMeta>>();
    private List<Stack<GameObject>> reserve = new List<Stack<GameObject>>();
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
    private void CreateInstanceHelper(Transform parent, Inst type, Vector3 position, bool isCell = false) {
        // Creates prefab instance
        try {
            active[(int)type].Add(new ChildMeta(parent, reserve[(int)type].Pop(), isCell));
            active[(int)type].Last().Object.SetActive(true);
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
            Debug.Log("Need more " + type);
        }
        // active[(int)type].Add(new ChildMeta(parent, reserve[(int)type].Pop(), isCell));
        Transform prefab = active[(int)type].Last().Object.transform;
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
        if (!first && !isPlatform && UnityEngine.Random.value <= gameScript.holeChance) {
            newLength = currFloorLength - 3; // 3 wide holes
            currFloor.localScale = new Vector3(newLength, 1, 1);
        }
        
        // =-=-=Main Structure Generation Loop=-=-=
        for (int i = 0; i < newLength; i++) {
            // =-=-=Cell Generation=-=-=
            CreateInstanceHelper(currFloor, Inst.cell, new Vector3(X + i, Y, 0), true);
            if (first) continue; // only generate cells for the first floor

            // =-=-=Spike Generation=-=-=
            if ((!isPlatform && spikeCoolDownIt == gameScript.spikeCoolDown) || (isPlatform && spikeCoolDownItPlat == gameScript.spikeCoolDown)) {
                // Randomly spawns spikes
                if (UnityEngine.Random.value <= gameScript.spikeChance && !spawnRandomPowerup) { // !spawnRandomPowerup do not spawn hazards when powerup should spawn
                    Vector3 spikePosition = new Vector3(X + i, Y + 1, 0);
                    // Randomly chooses which type of spike to spawn
                    float randomSpike = UnityEngine.Random.value;
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
                switch (UnityEngine.Random.Range(0, 4)) {
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
        lastLength = first ? 25 : UnityEngine.Random.Range(5, 26);
        active[(int)Inst.floor].Add(new ChildMeta(null, reserve[(int)Inst.floor].Pop()));
        active[(int)Inst.floor].Last().Object.SetActive(true);
        last = active[(int)Inst.floor].Last().Object.transform;
        last.position = new Vector3(X, Y, 0);
        last.localScale = new Vector3(lastLength, 1, 1); // edits floor's scale
        GenerateStructure(last, lastLength, X, Y, first); // generates structure
        floorLengths.Enqueue(new FloorMeta(last, lastLength));
        // Spawns platform
        if (!first && UnityEngine.Random.value <= 0.5f) {
            int randOffset = UnityEngine.Random.Range(lastValidX, lastLength);
            float platformX = X + randOffset;
            int platformY = Y + UnityEngine.Random.Range(3, 5);
            lastValidY = platformY - (platformY - Y) + 1;
            int platformLength = 5;
            lastValidX = (lastLength - 5 - randOffset < 0) ? 5 : 0;
            active[(int)Inst.floor].Add(new ChildMeta(null, reserve[(int)Inst.floor].Pop()));
            active[(int)Inst.floor].Last().Object.SetActive(true);
            Transform platform = active[(int)Inst.floor].Last().Object.transform;
            platform.position = new Vector3(platformX, platformY, 0);
            platform.localScale = new Vector3(platformLength, 1, 1);
            GenerateStructure(platform, platformLength, platformX, platformY, first, true);
            floorLengths.Enqueue(new FloorMeta(platform, platformLength));
        }
        else {
            lastValidY = 5; // Resets floor Y diversity
            lastValidX = 0; // Resets platform X restriction
        }
    }
    private GameObject InstantiateInactive(GameObject prefab, Vector3 position, string name) {
        GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
        obj.name = name;
        obj.SetActive(false);
        return obj;
    }

    public void Restart() {
        // Clears active and resets reserve
        foreach (Inst type in Enum.GetValues(typeof(Inst))) {
            HashSet<int> indicesToRemove = new HashSet<int>();
            for (int i = 0; i < active[(int)type].Count(); i++) {
                ChildMeta child = active[(int)type][i];
                reserve[(int)type].Push(child.Object);
                child.Object.transform.SetParent(transform, true);
                child.Object.SetActive(false);
                if (child.IsCell) child.Object.GetComponent<SpriteRenderer>().material = CellScript.normalMaterial;
                indicesToRemove.Add(i);
            }
            int index = 0;
            active[(int)type].RemoveAll(item => indicesToRemove.Contains(index++));
        }

        // Clears queue
        while (floorLengths.Count() > 0)
            floorLengths.Dequeue();

        // Spawns the first floor
        SpawnFloor();
        firstFloor = floorLengths.Dequeue();

        // Starts the powerup timer
        powerupTimer = gameScript.powerupSpawnCooldown;
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

        // Initializes active and reserve
        for (int i = 0; i < Enum.GetValues(typeof(Inst)).Length; i++) {
            active.Add(new List<ChildMeta>());
            reserve.Add(new Stack<GameObject>());
        }

        // Instantiates reserve
        for (int i = 1; i <= 10; i++) reserve[(int)Inst.floor].Push(InstantiateInactive(floor, new Vector3(-10, (int)Inst.floor, 0), $"{Inst.floor} {i}"));
        for (int i = 1; i <= 80; i++) reserve[(int)Inst.cell].Push(InstantiateInactive(cell, new Vector3(-10, (int)Inst.cell, 0), $"{Inst.cell} {i}"));
        for (int i = 1; i <= 21; i++) reserve[(int)Inst.singleSpike].Push(InstantiateInactive(singleSpike, new Vector3(-10, (int)Inst.singleSpike, 0), $"{Inst.singleSpike} {i}"));
        for (int i = 1; i <= 14; i++) reserve[(int)Inst.doubleSpike].Push(InstantiateInactive(doubleSpike, new Vector3(-10, (int)Inst.doubleSpike, 0), $"{Inst.doubleSpike} {i}"));
        for (int i = 1; i <= 12; i++) reserve[(int)Inst.tripleSpike].Push(InstantiateInactive(tripleSpike, new Vector3(-10, (int)Inst.tripleSpike, 0), $"{Inst.tripleSpike} {i}"));
        for (int i = 1; i <= 2; i++) reserve[(int)Inst.battery].Push(InstantiateInactive(battery, new Vector3(-10, (int)Inst.battery, 0), $"{Inst.battery} {i}"));
        for (int i = 1; i <= 2; i++) reserve[(int)Inst.shieldPrefab].Push(InstantiateInactive(shieldPrefab, new Vector3(-10, (int)Inst.shieldPrefab, 0), $"{Inst.shieldPrefab} {i}"));
        for (int i = 1; i <= 2; i++) reserve[(int)Inst.fuelPrefab].Push(InstantiateInactive(fuelPrefab, new Vector3(-10, (int)Inst.fuelPrefab, 0), $"{Inst.fuelPrefab} {i}"));
        for (int i = 1; i <= 2; i++) reserve[(int)Inst.extraJumpPrefab].Push(InstantiateInactive(extraJumpPrefab, new Vector3(-10, (int)Inst.extraJumpPrefab, 0), $"{Inst.extraJumpPrefab} {i}"));

        
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
        foreach (ChildMeta child in active[(int)Inst.floor])
            child.Object.transform.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

        // Creates floors outside of the playable area
        if (last.position.x + lastLength < rightEdge)
            SpawnFloor(last.position.x + lastLength, UnityEngine.Random.Range(0, lastValidY), false);
        
        // Deletes floors outside of the playable area
        if (firstFloor.Floor.position.x + firstFloor.Length < leftEdge) {
            // Destroy(firstFloor.Floor.gameObject);
            foreach (Inst type in Enum.GetValues(typeof(Inst))) {
                HashSet<int> indicesToRemove = new HashSet<int>();
                for (int i = 0; i < active[(int)type].Count(); i++) {
                    ChildMeta child = active[(int)type][i];
                    if (child.Parent == firstFloor.Floor || child.Object.transform == firstFloor.Floor) { // Object for floor, parent for everything else
                        reserve[(int)type].Push(child.Object);
                        child.Object.transform.SetParent(transform, true);
                        child.Object.SetActive(false);
                        if (child.IsCell) child.Object.GetComponent<SpriteRenderer>().material = CellScript.normalMaterial;
                        indicesToRemove.Add(i);
                    }
                }
                int index = 0;
                active[(int)type].RemoveAll(item => indicesToRemove.Contains(index++));
            }
            firstFloor = floorLengths.Dequeue();
        }
    }
}
