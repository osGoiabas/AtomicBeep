using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent (typeof (BoxCollider2D))]
public class ControladorFronteira : MonoBehaviour
{
    public  BoxCollider2D managerBox;
    private Transform player;
    public GameObject boundary;

    void Start() {
        managerBox = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    void Update() {
        ManageBoundary();
    }

    void ManageBoundary() {
        if (managerBox.bounds.min.x < player.position.x && player.position.x < managerBox.bounds.max.x &&
            managerBox.bounds.min.y < player.position.y && player.position.y < managerBox.bounds.max.y) {
            boundary.SetActive(true);
        } else {
            boundary.SetActive(false);
        }
    }
}
