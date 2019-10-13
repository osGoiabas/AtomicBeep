using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ControladorCâmera : MonoBehaviour {
    private BoxCollider2D cameraBox;
    private Transform player;

    void Start() {
        cameraBox = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    void Update() {
        FollowPlayer();
    }

    void FollowPlayer() {
            if (GameObject.Find("fronteira")) {
                transform.position = new Vector3(Mathf.Clamp(player.position.x, 
                                                            GameObject.Find("fronteira").GetComponent<BoxCollider2D>().bounds.min.x + cameraBox.size.x / 2, 
                                                            GameObject.Find("fronteira").GetComponent<BoxCollider2D>().bounds.max.x - cameraBox.size.x / 2),
                                                 Mathf.Clamp(player.position.y, 
                                                            GameObject.Find("fronteira").GetComponent<BoxCollider2D>().bounds.min.y + cameraBox.size.y / 2, 
                                                            GameObject.Find("fronteira").GetComponent<BoxCollider2D>().bounds.max.y - cameraBox.size.y / 2),
                                                 transform.position.z);
            }
        }
}
