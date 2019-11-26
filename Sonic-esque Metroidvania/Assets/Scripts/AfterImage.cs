using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    float ghostDelay = 0.1f;
    float ghostDelaySeconds = 0.1f;
    public GameObject ghost;
    public bool makeGhost = true;

    void Update()
    {
        if (makeGhost)
        {
            if (ghostDelaySeconds > 0)
            {
                ghostDelaySeconds -= Time.deltaTime;
            }
            else
            {
                //criar ghost image
                GameObject currentGhost = Instantiate(ghost, transform.position, transform.rotation);
                //currentGhost.transform.parent = transform.parent;
                currentGhost.transform.localScale = transform.parent.localScale;
                Sprite currentSprite = GetComponent<SpriteRenderer>().sprite;
                currentGhost.GetComponent<SpriteRenderer>().sprite = currentSprite;
                ghostDelaySeconds = ghostDelay;
                Destroy(currentGhost, 0.5f);
            }
        }
    }
}
