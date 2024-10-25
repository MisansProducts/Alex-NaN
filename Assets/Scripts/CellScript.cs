using UnityEngine;

public class CellScript : MonoBehaviour {
    public Sprite active;
    public Material glowMaterial;
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            GetComponent<SpriteRenderer>().sprite = active;

            GetComponent<SpriteRenderer>().material = glowMaterial;
        }
    }
}
