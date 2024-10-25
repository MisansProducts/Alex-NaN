using UnityEngine;

public class CellScript : MonoBehaviour {
    public Sprite active;
    public Material glowMaterial;
    private GameScript gameScript;
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            SpriteRenderer cellSprite = GetComponent<SpriteRenderer>();

            cellSprite.sprite = active;
            cellSprite.material = glowMaterial; // need to fix shader

            gameScript.UpdateScore();
        }
    }

    void Start() {
        gameScript = FindObjectOfType<GameScript>();
    }
}
