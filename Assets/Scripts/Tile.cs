using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] Color hoverColor;
    [SerializeField] Color highlightColor = Color.yellow;

    [Header("Other Layers")]
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask roadLayer;
    [SerializeField] float layerCheckRadius = 0.2f;

    private Color defaultColor;

    private SpriteRenderer sr;
    private Vector3 defaultScale;
    // private GameObject selectionIndicator;
    // private SpriteRenderer siSR;

    GameMaster gm;
    bool isWalkable = false;
    bool isMouseOver = false;

    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        sr = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
        defaultColor = sr.color;
        // ~selectionIndicator = transform.Find("SelectionIndicator").gameObject;
        // ~siSR = selectionIndicator.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        sr.color = isWalkable ? highlightColor : defaultColor;

        // ~siSR.enabled = isMouseOver;
    }

    public void SetMouseOver(bool mouseOver)
    {
        if (mouseOver && !isMouseOver)
        {
            // set gm's active tile to this tile
        }
        isMouseOver = mouseOver;
    }

    public void HandleMouseClick()
    {
        if (isWalkable)
        {
            if (gm == null) gm = FindObjectOfType<GameMaster>();
            if (gm == null) Debug.LogError("GameMaster not found");

            Unit selectedUnit = gm.GetSelectedUnit();
            if (selectedUnit != null)
            {
                selectedUnit.MoveToTile(this);
            }
        }
        else
        {
            if (gm.GameInputMode == GameInputMode.UnitActive)
            {
                gm.DeselectUnit();
            }
            else if (gm.GameInputMode == GameInputMode.Idle)
            {
                // gm.ShowStoreUI();
            }
        }
    }

    public bool IsSpawnable()
    {
        // isclear and gameInputMode is idle
        return IsClear() && gm.GameInputMode == GameInputMode.Idle;
    }

    public bool HasRoad()
    {
        return Physics2D.OverlapCircle(transform.position, layerCheckRadius, roadLayer);
    }

    public bool IsClear()
    {
        return !Physics2D.OverlapCircle(transform.position, layerCheckRadius, obstacleLayer);
    }

    public void Highlight()
    {
        // Debug.Log("Highlight tile: " + gameObject.name);
        isWalkable = true;
    }

    public void ResetTile()
    {
        // Debug.Log("Reset tile: " + gameObject.name);
        isWalkable = false;
    }

    // draw a gizmo matching the OverlapCircle of layerCheckRadius
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, layerCheckRadius);
    }
}
