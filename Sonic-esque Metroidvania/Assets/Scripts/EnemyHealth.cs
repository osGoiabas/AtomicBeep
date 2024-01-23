using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private bool podeLevarDano = true;
    private int totalHealth = 100;
    private float invulnerabilityTime = .2f;
    private bool isHit = false;
    private int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = totalHealth;
    }

    public void Damage(int amount)
    {
        if (podeLevarDano && !isHit && currentHealth > 0) {
            isHit = true;
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                FindObjectOfType<SoundManager>().PlaySFX("enemyDestroy");
                gameObject.SetActive(false);
            }
            else
            {
                FindObjectOfType<SoundManager>().PlaySFX("enemyHurt");
                StartCoroutine(TurnOffHit());
            }
        }
    }

    private IEnumerator TurnOffHit() {
        yield return new WaitForSeconds(invulnerabilityTime);
        isHit = false;
    }
}
