using System.Collections.Generic;
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
    private int floorNumber = 1;

    // Function to spawn floors
    private void SpawnFloor(float X = 0f, float Y = 0f, bool first = true) {
        print($"Floor number: {floorNumber}");
        floorNumber++;
        lastLength = first ? 25 : Random.Range(5, 25);
        last = Instantiate(floor, new Vector3(X, Y, 0), Quaternion.identity, transform).transform;
        last.localScale = new Vector3(lastLength, 1, 1); // edits floor's scale
        last.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // random color for testing

        floorLengths.Enqueue(new FloorMeta(last, lastLength));
    }

    // Awake is called before the game starts; used to initialize object references
    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
    }

    // Start is called before the first frame update
    void Start() {
        SpawnFloor();
        firstFloor = floorLengths.Dequeue();
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
            SpawnFloor(last.position.x + lastLength, Random.Range(1, 5), false);
        
        // Deletes floors outside of the playable area
        if (firstFloor.Floor.position.x + firstFloor.Length < leftEdge) {
            Destroy(firstFloor.Floor.gameObject);
            firstFloor = floorLengths.Dequeue();
        }
    }
}
