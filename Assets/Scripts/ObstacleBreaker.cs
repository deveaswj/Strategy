using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBreaker : MonoBehaviour
{
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float layerCheckRadius = 0.2f;

    private PlayerID playerID;
    public PlayerID PlayerID => playerID;

    void Start()
    {
        Unit unit = GetComponent<Unit>();
        playerID = unit.PlayerID;
    }

    public void BreakObstacles()
    {
        // if this gameobject is touching an obstacle that can be broken, break it
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, layerCheckRadius, obstacleLayer);
        foreach (Collider2D collider in colliders)
        {
            GameObject gameObject = collider.gameObject;
            Obstacle obstacle = gameObject.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                if (obstacle.Breakable)
                {
                    obstacle.ConvertToRubble();
                }
            }
        }
    }

}
