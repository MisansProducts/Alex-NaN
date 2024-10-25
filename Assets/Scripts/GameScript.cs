using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour
{
    public GameObject player;
    public GameObject floorSpawner;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(player, new Vector3(-6.5f, 0.5f, 0), Quaternion.identity, transform);
        Instantiate(floorSpawner, new Vector3(0, 0, 0), Quaternion.identity, transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
