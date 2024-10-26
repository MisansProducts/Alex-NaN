using UnityEngine;
using TMPro;

public class GameScript : MonoBehaviour
{
    #region Singleton

    public static GameScript Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    #endregion

    public GameObject player;
    public GameObject floorSpawner;
    [SerializeField] public float gameSpeed = 5f;
    [SerializeField] public float spikeChance = 0.2f;
    public float currentScore = 0;
    public int highScore = 0;
    public bool isInGame = false;

    // UI Text references
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI highScoreText;

    private void OnGUI(){
        currentScoreText.text = Mathf.RoundToInt(currentScore).ToString();
    }
    private void Update(){
        if (isInGame){
            currentScore += Time.deltaTime;
        }

        if (Input.GetKeyDown("space")){ //continue the game after game over
            if (!isInGame){
                isInGame = true;
            }
        }
    }
    public string displayScore(){
        return Mathf.RoundToInt(currentScore).ToString();
    }

    public void UpdateScore() {
        //highScore = currentScore > highScore ?  currentScore : highScore;
        // These don't work because drag and drop doesn't work! Print for now...
        // currentScoreText.text = "Score: " + currentScore.ToString();
        // highScoreText.text = "Hi-Score: " + highScore.ToString();
        print(currentScore);
        print(highScore);
    }

    public void GameOver() {
        isInGame = false;
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
