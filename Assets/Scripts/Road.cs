using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    [Header("Exits")]
#pragma warning disable IDE0044 // Add readonly modifier
    [SerializeField] bool exitNorth = false;
    [SerializeField] bool exitSouth = false;
    [SerializeField] bool exitEast = false;
    [SerializeField] bool exitWest = false;
#pragma warning restore IDE0044 // Add readonly modifier
}
