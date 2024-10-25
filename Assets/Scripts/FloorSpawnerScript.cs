using UnityEngine;

public class FloorSpawnerScript : MonoBehaviour {
    public GameObject floor; // Floor prefab
    public GameObject cell; // 1x1 cell prefab
    private Transform last; // Last floor spawned
    private const int floorLength = 25;
    private const int floorHeight = 1;
    [SerializeField] private float playerSpeed = 5f;
    private const float Y = -0.5f;
    private const float edgePadding = 2.5f;
    private float cameraLeftEdge;
    private float cameraRightEdge;

    // Function to spawn floors
    void SpawnFloor(float X = 0) {
        last = Instantiate(floor, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;

        // Fills floor with cell prefabs
        for (int x = 0; x < floorLength; x++) {
            for (int y = 0; y < floorHeight; y++) {
                Vector3 cellPosition = new Vector3(x - floorLength / 2, y, 0);
                GameObject cellPrefab = Instantiate(cell, cellPosition, Quaternion.identity, last);
                cellPrefab.transform.localPosition = cellPosition; // Local to last
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
            child.Translate(Vector3.left * playerSpeed * Time.deltaTime);

            // Deletes floors outside of the playable area
            if (child.position.x + floorLength / 2 < cameraLeftEdge)
                Destroy(child.gameObject);
        }
        
        // Spawns a new floor
        if (last.position.x + floorLength / 2 < cameraRightEdge)
            SpawnFloor(last.position.x + floorLength);
    }
}
