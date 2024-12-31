using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleDecor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // randomly flipX and/or flipY
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr != null) FlipSprite(sr);
        // if this object has any child objects with a SpriteRenderer component, flip them too
        foreach (Transform child in transform)
        {
            SpriteRenderer childSR = child.GetComponent<SpriteRenderer>();
            if (childSR != null) FlipSprite(childSR);
        }
    }

    void FlipSprite(SpriteRenderer sr)
    {
        if (sr == null) return;
        sr.flipX = (Random.Range(0f, 1f) > 0.5f);
        sr.flipY = (Random.Range(0f, 1f) > 0.5f);
        // rotate the sprite 0, 45, 90, 135, 180, 225, 270, or 315 degrees
        sr.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 8f) * 45f);
    }
}
