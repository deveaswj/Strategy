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
        transform.position = position;
    }
}
