using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    float ghostDelayTotal = 0.1f;
    float ghostDelaySeconds = 0f;
    public GameObject ghost;
    public bool makeGhost = false;

    void Update()
    {
        if (makeGhost)
        {
            if (ghostDelaySeconds > 0)
            {
                ghostDelaySeconds -= Time.unscaledDeltaTime;
            }
            else
            {
                //criar ghost image
                GameObject currentGhost = Instantiate(ghost, transform.position, transform.rotation);

                //currentGhost.transform.parent = transform.parent;
                currentGhost.transform.localScale = transform.parent.localScale;

                Sprite currentSprite = GetComponent<SpriteRenderer>().sprite;
                currentGhost.GetComponent<SpriteRenderer>().sprite = currentSprite;

                ghostDelaySeconds = ghostDelayTotal;

                Destroy(currentGhost, 0.5f);
            }
        }
    }
}
