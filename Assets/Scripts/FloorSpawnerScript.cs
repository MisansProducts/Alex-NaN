using UnityEngine;

public class FloorSpawnerScript : MonoBehaviour {
    public GameObject floor; // Floor prefab
    public GameObject cell; // 1x1 cell prefab
    public GameObject singleSpike, doubleSpike, tripleSpike; // Hazard
    private Transform last; // Last floor spawned
    private const int floorLength = 25;
    private const int floorHeight = 1;
    private const float Y = 0.5f;
    private const int leftEdge = -1; // 0 - 1 padding
    private const int rightEdge = 22; // 21 + 1 padding
    private GameScript gameScript;
    private int spikeCoolDown = 2;

    // Function to spawn floors
    void SpawnFloor(float X = 8f, bool first = true) {
        last = Instantiate(floor, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;
        

        // Fills floor with cell prefabs
        for (int x = 0; x < floorLength; x++) {
            for (int y = 0; y < floorHeight; y++) {
                Vector3 cellPosition = new Vector3(x - floorLength / 2, y, 0);
                GameObject cellPrefab = Instantiate(cell, cellPosition, Quaternion.identity, last);
                cellPrefab.transform.localPosition = cellPosition; // Local to last
            }

            // should be more rigorous later
            if (first) continue;
            if (spikeCoolDown == 2) {
                // Randomly spawns spikes
                if (Random.value <= gameScript.spikeChance) {
                    Vector3 spikePosition = new Vector3(x - 0.5f - floorLength / 2, floorHeight, 0);
                    float randomSpike = Random.value;
                    GameObject spikePrefab;
                    if (randomSpike <= 0.5f) {
                        spikePrefab = Instantiate(singleSpike, spikePosition, Quaternion.identity, last);
                        spikeCoolDown = 0;
                        spikePrefab.transform.localPosition = spikePosition;
                    }
                    else if (randomSpike <= 5f/6f && x < floorLength - 1) {
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

    // Start is called before the first frame update
    void Start() {
        gameScript = FindObjectOfType<GameScript>();
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

        if (last.position.x + floorLength / 2 < rightEdge)
        SpawnFloor(last.position.x + floorLength, false);
    }
}
