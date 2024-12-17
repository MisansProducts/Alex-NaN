using UnityEngine;

public class CellScript : MonoBehaviour {
    // Objects
    private GameScript gameScript;
    private SpriteRenderer cellSprite;
    public Sprite active;
    public Material glowMaterial;
    
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            //cellSprite.sprite = active;
            cellSprite.material = glowMaterial; // need to fix shader

            gameScript.UpdateScore();
        }
    }
    void Awake() {
        gameScript = FindObjectOfType<GameScript>();
        cellSprite = GetComponent<SpriteRenderer>();
    }
}
