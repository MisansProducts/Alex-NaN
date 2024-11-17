using JetBrains.Annotations;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

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
    private int spikeCoolDown = 2;
    private int elevationCoolDown = -20; 
    private const int floorLength = 25;
    private int floorHeight = 0;
    private const int platformLength = 3;
    private const int platformHeight = 1;
    private const float Y = 0.5f;
    private const float platformY = 2.5f;
    private const int leftEdge = -1; // 0 - 1 padding
    private const int rightEdge = 17; // 16 + 1 padding
    private const int holeSpawnCoolDown = 25;
    private int lastHole = -30;
    private const int holeLength = 4;
    private int holeSpawn = 0;
    private uint forceFloor = 0;
    [SerializeField] private float powerupSpawnCooldown = 20f; // Cooldown time for spawning powerups
    private float powerupTimer;
    private int maxFuelCount = 3;
    private int maxExtraJumpCount = 1;

    // Function to spawn floors
    void SpawnFloor(float X = 8f, bool first = true) {
        last = Instantiate(floor, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;

        // Fills floor with cell prefabs
        // 21 to 30
        // create object ? for platform : ternary
        for (int x = 0; x < floorLength; x++) {
            // =-=-=Hole Spawning=-=-=
            if(lastHole >= holeSpawnCoolDown && forceFloor == 0) {
                holeSpawn++;
                if(holeSpawn >= holeLength) {
                    holeSpawn = 0;
                    lastHole = 0;
                }
                continue;
            }
            else {
                if(forceFloor > 0) {
                    forceFloor--;
                }
                lastHole++;

                Vector3 cellPosition = new Vector3(x - floorLength / 2, floorHeight, 0); // (floorLength / 2) is floor center
                GameObject cellPrefab = Instantiate(cell, cellPosition, Quaternion.identity, last);
                cellPrefab.transform.localPosition = cellPosition; // Local to last
                cellPosition.y+= 1f;
                
                if (spikeCoolDown == 2 && Random.value <= 0.05) { // some random floating point idgaf
                    GameObject batteryPrefab = Instantiate(battery, cellPosition, Quaternion.identity, last);
                    batteryPrefab.transform.localPosition = cellPosition;
                    spikeCoolDown = 1; // so it doesn't spawn spike on top of it
                }
                // =-=-=Elevation Manipulation=-=-=
                if (elevationCoolDown == 10) {
                    if(Random.Range(0,1) == 0) {    // elevation has a 50% chance of changing each step after 10 steps; feel free to change
                        floorHeight = Random.Range(1, 5);
                        elevationCoolDown = 0;
                        continue; // prevent spikes being spawned on edges of cliffs
                        }
                }
                else {
                    elevationCoolDown++;
                }

                // =-=-=Spike Generation=-=-=
                if (first || (gameScript.spotLightScore - gameScript.currentScore > 0 && gameScript.spotLightScore - gameScript.currentScore < 20)) continue; // skips spawning spikes
                if (spikeCoolDown == 2) {
                    // Randomly spawns spikes
                    if (Random.value <= gameScript.spikeChance) {
                        Vector3 spikePosition = new Vector3(x - 0.5f - floorLength / 2, floorHeight + 1, 0);
                        float randomSpike = Random.value;
                        GameObject spikePrefab;
                        if (randomSpike <= 1f/2f) {
                            spikePrefab = Instantiate(singleSpike, spikePosition, Quaternion.identity, last);
                            spikeCoolDown = 0;
                            spikePrefab.transform.localPosition = spikePosition;
                        }
                        else if (randomSpike <= 5f/6f) {
                            if (x >= floorLength - 1) continue;
                            spikePrefab = Instantiate(doubleSpike, spikePosition, Quaternion.identity, last);
                            spikeCoolDown = -1;
                            spikePrefab.transform.localPosition = spikePosition;
                            forceFloor = 1;
                        }
                        else if (x < floorLength - 2) {
                            spikePrefab = Instantiate(tripleSpike, spikePosition, Quaternion.identity, last);
                            spikeCoolDown = -2;
                            spikePrefab.transform.localPosition = spikePosition;
                            forceFloor = 2;
                        }
                    }
                }
                else { // just some basic logic to make the game fair
                    spikeCoolDown++;
                }
            }
        }
    }

    // Function to spawn floating platforms
    void spawnPlatForm(float platX = 3f, bool first = true) {
        last = Instantiate(floor, new Vector3(platX, platformY, 0), Quaternion.identity, transform).transform;

        // Fills floor with cell prefabs
        for (int x = 0; x < platformLength; x++) {
            for (int y = 0; y < platformHeight; y++) {
                Vector3 cellPosition = new Vector3(x - platformLength / 2, y, 0); // (Length / 2) is floor center
                GameObject cellPrefab = Instantiate(cell, cellPosition, Quaternion.identity, last);
                cellPrefab.transform.localPosition = cellPosition; // Local to last
                cellPosition.y += 1f;
                if (Random.value <= 0.1) { // another random floating point, higher for floating platforms
                    GameObject batteryPrefab = Instantiate(battery, cellPosition, Quaternion.identity, last);
                    batteryPrefab.transform.localPosition = cellPosition;
                }
            }
        }
    }

    private void HandlePowerupSpawning() {
        powerupTimer -= Time.deltaTime;

        if (powerupTimer <= 0) {
            SpawnRandomPowerup();
            powerupTimer = powerupSpawnCooldown; // Reset the timer
        }
    }

    private void SpawnRandomPowerup() {
        // Find all the cells currently in the scene
        GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        if (cells.Length == 0) return; // No cells to spawn on

        // Filter cells to only those to the right of the player
        Vector3 playerPosition = FindObjectOfType<PlayerScript>().transform.position;
        List<GameObject> validCells = new List<GameObject>();
        foreach (GameObject cell in cells) {
            if (cell.transform.position.x > playerPosition.x) {
                validCells.Add(cell);
            }
        }

        if (validCells.Count == 0) return; // No valid cells to spawn on

        // Randomly choose a valid cell to spawn the powerup on
        GameObject selectedCell = validCells[Random.Range(0, validCells.Count)];
        Vector3 spawnPosition = selectedCell.transform.position + Vector3.up; // Spawn above the cell

        // Randomly choose a powerup to spawn
        int powerupType = Random.Range(0, 3);

        GameObject powerupInstance = null;
        switch (powerupType) {
            case 0: // Fuel
                if (playerScript.fuelCount < maxFuelCount) {
                    powerupInstance = Instantiate(fuelPrefab, spawnPosition, Quaternion.identity, selectedCell.transform);
                }
                break;
            case 1: // ExtraJump
                if (playerScript.extraJumpCount < maxExtraJumpCount) {
                    powerupInstance = Instantiate(extraJumpPrefab, spawnPosition, Quaternion.identity, selectedCell.transform);
                }
                break;
            case 2: // Shield
                powerupInstance = Instantiate(shieldPrefab, spawnPosition, Quaternion.identity, selectedCell.transform);
                break;
        }

        if (powerupInstance != null) {
            powerupInstance.transform.parent = selectedCell.transform; // Attach powerup to the cell
        }
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
        SpawnFloor();
        powerupTimer = powerupSpawnCooldown;
    }

    // Update is called once per frame
    void Update() {
        // Moves floors to the left
        foreach (Transform child in transform) {
        child.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

        // Deletes floors outside of the playable area
        if (child.position.x + floorLength / 2 < leftEdge)
            Destroy(child.gameObject);
        }

        if (last.position.x + floorLength / 2 < rightEdge){
            SpawnFloor(last.position.x + floorLength, false);
            //spawnPlatForm(last.position.x + platformLength, false);
        }

        HandlePowerupSpawning();
    }

    // returns the value of floor height 
    // used to determine the y-value of floating platforms
    public int getFloorHeight() {
        int height = floorHeight;    
        return height;
    }
}
