using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableResource : MonoBehaviour
{
    [SerializeField] private int value = 1;

    public int GetValue() => value;
}
