using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;

public class FloorSpawnerScript : MonoBehaviour {

    // Objects
    private GameScript gameScript;
    private Transform last; // Last floor spawned
    [SerializeField] private GameObject floor; // Floor prefab

    [SerializeField] private GameObject cell; // 1x1 cell prefab
    [SerializeField] private GameObject singleSpike, doubleSpike, tripleSpike; // Hazards
    [SerializeField] private GameObject battery;
    
    // Variables
    private int floorLength = 25;
    private const float Y = 0.5f;
    private const int leftEdge = -1; // 0 - 1 padding
    private const int rightEdge = 17; // 16 + 1 padding

    // Function to spawn floors
    private void SpawnFloor(float X = 8f, bool first = true) {
        print("WOW! A FLOOR!!!");
        floorLength = Random.Range(5,15);
        last = Instantiate(floor, new Vector3(X, Random.Range(1,5), 0), Quaternion.identity, transform).transform;
        last.localScale = new Vector3(floorLength, 1, 1); // edits floor's scale
        last.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f); // random color for testing
        
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
        
    }

    // Same as Update() but more consistent
    void FixedUpdate() {
        // Moves floors to the left
        foreach (Transform child in transform) {
            child.Translate(Vector3.left * gameScript.gameSpeed * Time.deltaTime);

            // Deletes floors outside of the playable area
            if (child.position.x + 15 < leftEdge)
                Destroy(child.gameObject);
        }

        //Maybe useful for a different game mode
        //floorLength = Random.Range(5,15);

        if (last.position.x + floorLength / 2 < rightEdge){
            SpawnFloor(last.position.x + floorLength, false);
        }
    }
}
