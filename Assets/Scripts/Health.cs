using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public PlayerMovement player;
    public int numOfHearts = 5;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Start () {
        player = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        for (int i = 0; i < hearts.Length; i++) {
            if (i < player.vida) {
                hearts[i].sprite = fullHeart;
            } else {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}
