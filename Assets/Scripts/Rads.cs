using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rads : MonoBehaviour
{
    private int radValue = 1;

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) 
        {
            FindFirstObjectByType<ItemManager>().ChangeRadsCollected(radValue);
            FindFirstObjectByType<SoundManager>().PlaySFX("rads");
            Destroy(gameObject);
        }
    }
}
