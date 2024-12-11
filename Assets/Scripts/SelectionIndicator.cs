using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    [SerializeField] Sprite selectionSprite;
    [SerializeField] Sprite wrenchSprite;

    [SerializeField] LayerMask tileLayer; // Set this to the Tile layer in the Inspector
    [SerializeField] LayerMask unitLayer; // Set this to the Unit layer in the Inspector

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
    }

    public bool TryMove(Vector2 direction)
    {
        bool success = false;
        Vector3 targetPosition = OffsetPosition(direction);
        if (Physics2D.OverlapCircle(targetPosition, 0.1f, tileLayer) != null)
        {
            MoveTo(targetPosition);
            success = true;
        }
        return success;
    }

    public void MoveTo(Vector2 position)
    {
        // move this object to the given position
        Debug.Log("Moving selection indicator to " + position);
        transform.position = position;
    }

    public Vector3 OffsetPosition(Vector3 offset)
    {
        return transform.position + offset;
    }

    public Vector3 OffsetPosition(Vector2 offset) => OffsetPosition(new Vector3(offset.x, offset.y, 0));
    public Vector3 OffsetPosition(float x, float y, float z = 0) => OffsetPosition(new Vector3(x, y, z));

}
