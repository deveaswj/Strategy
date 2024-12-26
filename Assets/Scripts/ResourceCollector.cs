using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
    [SerializeField] LayerMask rubbleLayer;
    [SerializeField] float layerCheckRadius = 0.2f;

    private PlayerID playerID;
    public PlayerID PlayerID => playerID;

    void Start()
    {
        Unit unit = GetComponent<Unit>();
        playerID = unit.PlayerID;
    }

    public int Collect()
    {
        int amount = 0;
        // if this gameobject is touching rubble, collect it (destroy it)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, layerCheckRadius, rubbleLayer);
        foreach (Collider2D collider in colliders)
        {
            GameObject gameObject = collider.gameObject;
            CollectableResource collectableResource = gameObject.GetComponent<CollectableResource>();
            if (collectableResource != null)
            {
                amount += collectableResource.GetValue();
            }
            Destroy(gameObject);
        }
        return amount;
    }

}
