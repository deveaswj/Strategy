using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustCloud : MonoBehaviour
{

    [SerializeField] [Range(0, 1)] int direction; // 0 to get smaller/fade out, 1 to get bigger/fade in
    [SerializeField] float speed = 1f;
    SpriteRenderer sr;
    float spriteAlpha;
    float spriteSize;
    [SerializeField] float alphaMin = 0.25f;
    [SerializeField] float alphaMax = 0.5f;
    [SerializeField] float sizeMin = 0.65f;
    [SerializeField] float sizeMax = 1f;
    [SerializeField] Color[] colors = { Color.white };

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Color length = " + colors.Length);

        Debug.Log("Starting direction = " + direction);
        sr = GetComponent<SpriteRenderer>();
        spriteAlpha = (alphaMin + alphaMax) / 2f;
        spriteSize = (sizeMin + sizeMax) / 2f;
        // spriteAlpha = direction == 0 ? alphaMax : alphaMin;
        // spriteSize = direction == 0 ? sizeMax : sizeMin;
        UpdateSpriteAlpha();
        UpdateSpriteSize();
        Color newColor = PickRandomColor();
        sr.color = newColor;
    }

    void UpdateSpriteAlpha()
    {
        Color color = sr.color;
        color.a = spriteAlpha;
        sr.color = color;
    }

    void UpdateSpriteSize()
    {
        transform.localScale = new Vector3(spriteSize, spriteSize, transform.localScale.z);
    }

    Color PickRandomColor()
    {
        // choose a random color
        int index = Random.Range(0, colors.Length);
        Debug.Log("Index: " + index + ", Length: " + colors.Length);
        Color newColor = colors[index];
        newColor.a = spriteAlpha;
        return newColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (direction == 1)
        {
            // Fading in
            spriteAlpha = Mathf.Lerp(spriteAlpha, alphaMax, speed * Time.deltaTime);
            spriteSize = Mathf.Lerp(spriteSize, sizeMax, speed * Time.deltaTime);
            if (Mathf.Abs(spriteAlpha - alphaMax) < 0.01f)
            {
                spriteAlpha = alphaMax;
                direction = 0; // Reverse direction
            }
        }
        else
        {
            // Fading out
            spriteAlpha = Mathf.Lerp(spriteAlpha, alphaMin, speed * Time.deltaTime);
            spriteSize = Mathf.Lerp(spriteSize, sizeMin, speed * Time.deltaTime);
            if (Mathf.Abs(spriteAlpha - alphaMin) < 0.01f)
            {
                spriteAlpha = alphaMin;

                // rotate 0, 90, 180, or 270 degrees
                float newRotation = Random.Range(0, 4) * 90f;
                transform.Rotate(0f, 0f, newRotation);
                Debug.Log(gameObject.name + " rotated by " + newRotation + " to: " + transform.rotation);

                // choose a random color
                Color newColor = PickRandomColor();
                sr.color = newColor;

                direction = 1; // Reverse direction
            }
        }

        UpdateSpriteAlpha();
        UpdateSpriteSize();
    }
}
