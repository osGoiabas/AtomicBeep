using UnityEngine;

public class Mosca : MonoBehaviour
{
    public float speed = 1f;
    public Transform beep;

    void Update()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, 
                                              beep.position,
                                              speed * Time.deltaTime);
    }
}
