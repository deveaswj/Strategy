using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    [SerializeField] Sprite selectionSprite;
    [SerializeField] Sprite targetingSprite;
    [SerializeField] Color targetingColor = Color.red;
    [SerializeField] Color defaultColor = Color.white;

    public void MoveTo(Vector2 position)
    {
        // move this object to the given position
        Debug.Log("Moving selection indicator to " + position);
        transform.position = position;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }
}
