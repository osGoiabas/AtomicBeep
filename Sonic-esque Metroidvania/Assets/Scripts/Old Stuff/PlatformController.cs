using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController {

    public override void Start() {
        base.Start();
    }

    public Vector3 move;

    void FixedUpdate() {
        Vector3 velocity = move * Time.deltaTime;
        transform.Translate (velocity);
    }

    void MovePassangers(Vector3 velocity) { 
            
    }
}
