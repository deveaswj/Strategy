using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool selected;
    GameMaster gm;

    // List<Tile> walkableTiles = new List<Tile>();

    public int tileSpeed;   // How many tiles the unit moves per turn
    public bool hasMoved;

    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
    }

    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        // gm.SelectUnit(this);
        if (selected)
        {
            selected = false;
            gm.selectedUnit = null;
        }
        else
        {
            if (gm.selectedUnit != null)
            {
                gm.selectedUnit.selected = false;
            }
            selected = true;
            gm.selectedUnit = this;
            GetWalkableTiles();
        }
    }

    void GetWalkableTiles()
    {
        if (hasMoved) return;

        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            float distanceX = Mathf.Abs(tile.transform.position.x - transform.position.x);
            float distanceY = Mathf.Abs(tile.transform.position.y - transform.position.y);
            if (distanceX + distanceY <= tileSpeed)
            {
                if (tile.IsClear())
                {
                    tile.Highlight();
                    // walkableTiles.Add(tile);
                }
            }
        }
    }
}
