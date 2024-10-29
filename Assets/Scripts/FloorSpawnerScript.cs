using UnityEngine;

public class FloorSpawnerScript : MonoBehaviour {
    public GameObject floor; // Floor prefab
    public GameObject cell; // 1x1 cell prefab
    public GameObject spike; // Hazard
    private Transform last; // Last floor spawned
    private const int floorLength = 25;
    private const int floorHeight = 1;
    private const float Y = 0.5f;
    private const float edgePadding = 2.5f;
    private float cameraLeftEdge;
    private float cameraRightEdge;
    private GameScript gameScript;
    private int spikeLimit = 3;
    private int spikeCoolDown = 2;

    // Function to spawn floors
    void SpawnFloor(float X = 0, bool first = true) {
        gameScript = FindObjectOfType<GameScript>();
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
                    spikeLimit -= 1;
                    if (spikeLimit == 0) {
                        spikeLimit = 3;
                        spikeCoolDown = 0;
                    }
                    Vector3 spikePosition = new Vector3(x - floorLength / 2, floorHeight, 0);
                    GameObject spikePrefab = Instantiate(spike, spikePosition, Quaternion.identity, last);
                    spikePrefab.transform.localPosition = spikePosition;
                }
                else {
                    spikeLimit = 3;
                    spikeCoolDown = 1;
                }
            }
            else { // just some basic logic to make the game fair
                spikeCoolDown++;
            }
        }
    }

    // Start is called before the first frame update
    void Start() {
        SpawnFloor();
        cameraLeftEdge = -Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - edgePadding;
        cameraRightEdge = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x + edgePadding;
    }

    // Update is called once per frame
    void Update() {
        // Moves floors to the left
        foreach (Transform child in transform) {
        child.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

        // Deletes floors outside of the playable area
        if (child.position.x + floorLength / 2 < cameraLeftEdge)
            Destroy(child.gameObject);
        }

        if (last.position.x + floorLength / 2 < cameraRightEdge)
        SpawnFloor(last.position.x + floorLength, false);
    }
}
