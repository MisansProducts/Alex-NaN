using UnityEngine;

public class platformSpawnerScript : MonoBehaviour {

    // Objects
    private GameScript gameScript;
    private FloorSpawnerScript floorScript;
    private Transform last; // Last floor spawned
    [SerializeField] private GameObject platform; // Floor prefab

    [SerializeField] private GameObject cell; // 1x1 cell prefab
    [SerializeField] private GameObject singleSpike, doubleSpike, tripleSpike; // Hazards
    
    // Variables
    private int spikeCoolDown = 2;
    private int platLength = 5;
    private int platHeight = 3;
    private const float Y = 0.5f;
    private const int leftEdge = -1; // 0 - 1 padding
    private const int rightEdge = 17; // 16 + 1 padding

    // Function to spawn floors
    void SpawnPlatform(float X = 8f, bool first = true, int floorHeight = 1) {
        last = Instantiate(platform, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;
        platHeight = floorHeight + 2;

        // Fills floor with cell prefabs
        // 21 to 30
        // create object ? for platform : ternary
        for (int x = 0; x < platLength; x++) {
                Vector3 cellPosition = new Vector3(x - platLength / 2, platHeight, 0); // (floorLength / 2) is floor center
                GameObject cellPrefab = Instantiate(cell, cellPosition, Quaternion.identity, last);
                cellPrefab.transform.localPosition = cellPosition; // Local to last


            // =-=-=Spike Generation=-=-=
            if (first || (gameScript.spotLightScore - gameScript.currentScore > 0 && gameScript.spotLightScore - gameScript.currentScore < 20)) continue; // skips spawning spikes
            if (spikeCoolDown == 2) {
                // Randomly spawns spikes
                if (Random.value <= gameScript.spikeChance) {
                    Vector3 spikePosition = new Vector3(x - 0.5f - platLength / 2, platLength, 0);
                    float randomSpike = Random.value;
                    GameObject spikePrefab;
                    if (randomSpike <= 1f/2f) {
                        spikePrefab = Instantiate(singleSpike, spikePosition, Quaternion.identity, last);
                        spikeCoolDown = 0;
                        spikePrefab.transform.localPosition = spikePosition;
                    }
                    else if (randomSpike <= 5f/6f) {
                        if (x >= platLength - 1) continue;
                        spikePrefab = Instantiate(doubleSpike, spikePosition, Quaternion.identity, last);
                        spikeCoolDown = -1;
                        spikePrefab.transform.localPosition = spikePosition;
                    }
                    else if (x < platLength - 2) {
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

    // Awake is called before the game starts; used to initialize object references
    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        floorScript = FindObjectOfType<FloorSpawnerScript>();
    }

    // Start is called before the first frame update
    void Start() {
        SpawnPlatform();
    }

    // Update is called once per frame
    void Update() {
        // Moves floors to the left
        foreach (Transform child in transform) {
        child.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

        // Deletes floors outside of the playable area
        if (child.position.x + platLength / 2 < leftEdge)
            Destroy(child.gameObject);
        }

        // if (last.position.x + platLength / 2 < rightEdge)
        // SpawnPlatform(last.position.x + platLength, false);

        // Random chance of spawning platforms
        if (Random.Range(0,5) == 0) {
            int floorHeight = floorScript.getFloorHeight();
            SpawnPlatform(last.position.x + platLength, false, floorHeight);
        }
    }
}
