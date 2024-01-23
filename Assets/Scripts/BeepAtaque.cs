using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeepAtaque : MonoBehaviour
{
    private int damageAmount = 60;
    private float timerAtaque = 0.1f;
    private PlayerMovement character;
    private BoxCollider2D hitbox;

    private void Start()
    {
        hitbox = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyHealth>())
        {
            HandleCollision(collision.GetComponent<EnemyHealth>());
        }
    }

    private void HandleCollision(EnemyHealth objHealth)
    {
        objHealth.Damage(damageAmount);
        StartCoroutine(NoLongerColliding());
    }

    private IEnumerator NoLongerColliding() {
        yield return new WaitForSeconds(timerAtaque);
    }
}
