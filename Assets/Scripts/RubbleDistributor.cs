using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleDistributor : MonoBehaviour
{
    // this script runs at the game start

    // check every tile -- if it's clear, spawn rubble
    // "clear" means there's no unit, obstacle, or rubble on the tile already
    // also there should be a lower chance of rubble if there's a road on the tile

    [System.Serializable]
    public class RubbleType
    {
        public GameObject prefab;
        [Range (0f, 1f)] public float chance;
    }

    [SerializeField] [Range (0f, 1f)] float rubbleChance = 0.1f;
    // define our array of rubble prefabs
    [SerializeField] RubbleType[] rubbleTypes;
    [SerializeField] Transform rubbleParent;

    GameMaster gm;

    private float lastTotalOdds = 0f;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        CreateMapRubble();
    }

    void CreateMapRubble()
    {
        foreach (Tile tile in gm.GetTiles())
        {
            float spawnChance = Random.Range(0f, 1f);
            if (spawnChance > rubbleChance) continue;

            if (tile.IsClear() && !tile.HasRoad())
            {
                RubbleType rubbleType = GetRandomRubble();
                if (rubbleType == null) continue;

                GameObject prefab = rubbleType.prefab;
                // spawn rubble on this tile
                GameObject rubble = Instantiate(prefab, 
                                                tile.transform.position, 
                                                Quaternion.identity, 
                                                rubbleParent);
            }
        }
    }

    RubbleType GetRandomRubble()
    {
        float chance = Random.Range(0f, 1f);
        float cumulativeChance = 0;
        foreach (RubbleType rubbleType in rubbleTypes)
        {
            cumulativeChance += rubbleType.chance;
            if (chance < cumulativeChance) return rubbleType;
        }
        return null;
    }

    private void OnValidate()
    {
        float totalOdds = 0f;
        foreach (RubbleType rubbleType in rubbleTypes)
        {
            totalOdds += rubbleType.chance;
        }

        if (lastTotalOdds != totalOdds)
        {
            if (Mathf.Abs(totalOdds - 1f) > 0.01f)
            {
                Debug.LogWarning("Total odds should sum to 1.0!");
            }
            else
            {
                Debug.Log("Total odds: " + totalOdds);
            }
            lastTotalOdds = totalOdds;
        }
    }

}
