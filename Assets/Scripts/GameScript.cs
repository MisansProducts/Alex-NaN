using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;

public class GameScript : MonoBehaviour {
    public GameObject player;
    public GameObject floorSpawner;
    public Light2D spotLight;
    [SerializeField] public float gameSpeed = 5f;
    [SerializeField] public float spikeChance = 0.2f;
    public int currentScore = 0;
    public int highScore = 0;
    private const float spotLightRedScore = 200f; // Score in which spotlight turns fully red

    // UI Text references
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;

    public void UpdateScore() {
        currentScore++;
        highScore = currentScore > highScore ? currentScore : highScore;
        currentScoreText.text = "Score: " + currentScore.ToString();
        highScoreText.text = "Hi-Score: " + highScore.ToString();
        spotLight.color = Color.HSVToRGB(0, currentScore / spotLightRedScore * 1f, 1f);
    }

    public void GameOver() {
        gameSpeed = 0f;
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameSpeed = 5f;
        currentScore = 0;
        spotLight.color = Color.HSVToRGB(0, 0, 1f);

        Instantiate(player, new Vector3(-6.5f, 0.5f, 0), Quaternion.identity, transform);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
    }
    
    // Start is called before the first frame update
    void Start() {
        spotLight = GameObject.FindGameObjectWithTag("Light").GetComponent<Light2D>();
        Instantiate(player, new Vector3(-6.5f, 0.5f, 0), Quaternion.identity, transform);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
    }
}
