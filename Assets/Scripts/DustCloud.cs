using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustCloud : MonoBehaviour
{

    SpriteRenderer sr;
    float spriteAlpha;
    float maxSize = 0.65f;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        spriteAlpha = sr.color.a;
    }

    // Update is called once per frame
    void Update()
    {
        // Simulate the dust of work occurring on thhs tile:
        // grow to maxSize and fade in to half opacity
        // shrink to 0.5 and fade out to invisible
        // rotate 22.5 degrees
        // choose a new maxSize between 0.65 and 1
        // repeat

        // spriteAlpha ranges from 0.5 to 0f

        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, spriteAlpha);
        if (spriteAlpha < 0.5f)
        {
            sr.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));
        }


    }
}
