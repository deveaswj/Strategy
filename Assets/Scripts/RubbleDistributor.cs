using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleDistributor : MonoBehaviour
{
    // this script runs at the game start

    // check every tile -- if it's clear, spawn rubble
    // "clear" means there's no unit, obstacle, or rubble on the tile already
    // also there should be a lower chance of rubble if there's a road on the tile

    // define our array of rubble prefabs
    [SerializeField] GameObject[] rubblePrefabs;
    [SerializeField] Transform rubbleParent;

    GameMaster gm;


    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
    }

    void CreateMapRubble()
    {
        foreach (Tile tile in gm.GetTiles())
        {

            // randomly decide whether to spawn rubble
            float chance = Random.Range(0f, 1f);
            if (chance > 0.5f) continue;

            if (tile.IsClear())
            {
                // spawn rubble on this tile
                GameObject rubble = Instantiate(rubblePrefabs[Random.Range(0, rubblePrefabs.Length)], 
                                                tile.transform.position, 
                                                Quaternion.identity, 
                                                rubbleParent);
            }
        }
    }

}
