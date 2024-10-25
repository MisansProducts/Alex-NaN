using UnityEngine;

public class SquareScript : MonoBehaviour {
    public Sprite active;
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            GetComponent<SpriteRenderer>().sprite = active;
        }
    }
}
