using UnityEngine;

public class CellScript : MonoBehaviour {
    public Sprite active;
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            GetComponent<SpriteRenderer>().sprite = active;
        }
    }
}
