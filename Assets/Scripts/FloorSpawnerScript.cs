using UnityEngine;

public class FloorSpawnerScript : MonoBehaviour {

    // Objects
    private GameScript gameScript;
    private Transform last; // Last floor spawned
    [SerializeField] private GameObject floor; // Floor prefab
    [SerializeField] private GameObject cell; // 1x1 cell prefab
    [SerializeField] private GameObject singleSpike, doubleSpike, tripleSpike; // Hazards
    [SerializeField] private GameObject battery;
    
    // Variables
    private int spikeCoolDown = 2;
    private const int floorLength = 25;
    private const int floorHeight = 1;
    private const int platformLength = 3;
    private const int platformHeight = 1;
    private const float Y = 0.5f;
    private const float platformY = 2.5f;
    private const int leftEdge = -1; // 0 - 1 padding
    private const int rightEdge = 17; // 16 + 1 padding

    // Function to spawn floors
    void SpawnFloor(float X = 8f, bool first = true) {
        last = Instantiate(floor, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;

        // Fills floor with cell prefabs
        for (int x = 0; x < floorLength; x++) {
            for (int y = 0; y < floorHeight; y++) {
                Vector3 cellPosition = new Vector3(x - floorLength / 2, y, 0); // (floorLength / 2) is floor center
                GameObject cellPrefab = Instantiate(cell, cellPosition, Quaternion.identity, last);
                cellPrefab.transform.localPosition = cellPosition; // Local to last
                cellPosition.y+= 1f;
                if (Random.value <= 0.05) { // some random floating point idgaf
                    GameObject batteryPrefab = Instantiate(battery, cellPosition, Quaternion.identity, last);
                    batteryPrefab.transform.localPosition = cellPosition;
                    spikeCoolDown = 1; // so it doesn't spawn spike on top of it
                }
            }

            // =-=-=Spike Generation=-=-=
            if (first || (gameScript.spotLightScore - gameScript.currentScore > 0 && gameScript.spotLightScore - gameScript.currentScore < 20)) continue; // skips spawning spikes
            if (spikeCoolDown == 2) {
                // Randomly spawns spikes
                if (Random.value <= gameScript.spikeChance) {
                    Vector3 spikePosition = new Vector3(x - 0.5f - floorLength / 2, floorHeight, 0);
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
                    }
                    else if (x < floorLength - 2) {
                        spikePrefab = Instantiate(tripleSpike, spikePosition, Quaternion.identity, last);
                        spikeCoolDown = -2;
                        spikePrefab.transform.localPosition = spikePosition;
                    }
                }
            }
            else { // just some basic logic to make the game fair
                spikeCoolDown++;
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

    // Awake is called before the game starts; used to initialize object references
    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
    }

    // Start is called before the first frame update
    void Start() {
        SpawnFloor();
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
            spawnPlatForm(last.position.x + platformLength, false);
        }
    }
}
