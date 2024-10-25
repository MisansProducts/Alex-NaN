using UnityEngine;
using TMPro;

public class GameScript : MonoBehaviour
{
    public GameObject player;
    public GameObject floorSpawner;
    [SerializeField] public float gameSpeed = 5f;
    [SerializeField] public float spikeChance = 0.2f;
    public int currentScore = 0;
    public int highScore = 0;

    // UI Text references
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;

    public void UpdateScore() {
        currentScore += 1;
        highScore = currentScore > highScore ?  currentScore : highScore;
        // These don't work because drag and drop doesn't work! Print for now...
        // currentScoreText.text = "Score: " + currentScore.ToString();
        // highScoreText.text = "Hi-Score: " + highScore.ToString();
        print(currentScore);
        print(highScore);
    }

    public void GameOver() {
        gameSpeed = 0f;
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameSpeed = 5f;
        currentScore = 0;
        Instantiate(player, new Vector3(-6.5f, 0.5f, 0), Quaternion.identity, transform);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(player, new Vector3(-6.5f, 0.5f, 0), Quaternion.identity, transform);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
    }
}
