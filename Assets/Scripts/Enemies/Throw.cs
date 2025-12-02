using UnityEngine;

public class Throw : MonoBehaviour
{
    void Start()
    {
        // // throw the trash
        // this.GetComponent<Rigidbody2D>().AddForce(new Vector2(-200f, 200f));
    }

    void Update()
    {
        // rotate the trash
        this.transform.Rotate(0f, 0f, 300f * Time.deltaTime);
    }
}
