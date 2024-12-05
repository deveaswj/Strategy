using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    public void MoveTo(Vector2 position)
    {
        // move this object to the given position
        transform.position = position;
    }
}
