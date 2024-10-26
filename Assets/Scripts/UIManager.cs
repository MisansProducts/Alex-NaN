using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreUI;
    GameScript gs;
    private void start()
    {
        gs = GameScript.Instance;
    }

    private void Update()
    {
        //scoreUI.text = "Score: " + gs.displayScore();
    }
}
