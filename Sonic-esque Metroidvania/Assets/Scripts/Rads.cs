using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rads : MonoBehaviour
{
    [SerializeField] private int radValue = 1;
    [SerializeField] private AudioClip radSound;

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) 
        {
            ItemManager.instance.ChangeRadsCollected(radValue);
            AudioSource.PlayClipAtPoint(radSound, transform.position);
            Destroy(this.gameObject);
        }
    }
}
