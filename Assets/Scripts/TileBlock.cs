using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBlock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Rename each child object by appending *this* object's name to it.
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.name = child.name + " (" + transform.name + ")";
        }
    }
}
