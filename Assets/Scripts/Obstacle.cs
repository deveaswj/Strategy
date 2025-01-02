using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] GameObject rubblePrefab;
    [SerializeField] bool breakable = false;
    public bool Breakable => breakable;

    public void ConvertToRubble()
    {
        if (Breakable && GenerateRubble())
        {
            Destroy(gameObject);
        }
    }

    public bool GenerateRubble()
    {
        if (rubblePrefab == null) return false;
        GameObject rubble = Instantiate(rubblePrefab, transform.position, Quaternion.identity);
        return (rubble != null);
    }
}
