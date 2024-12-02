using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // [SerializeField] float hoverAmount = 0.1f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] Color hoverColor;
    [SerializeField] Color highlightColor = Color.yellow;
    [SerializeField] float isClearRadius = 0.2f;
    [SerializeField] bool isRoadTile = false;
    private Color defaultColor;

    private SpriteRenderer sr;
    private Vector3 defaultScale;
    private GameObject selectionIndicator;
    private SpriteRenderer siSR;

    GameMaster gm;
    bool isWalkable = false;
    bool isMouseOver = false;

    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        sr = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
        defaultColor = sr.color;
        selectionIndicator = transform.Find("SelectionIndicator").gameObject;
        siSR = selectionIndicator.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        sr.color = isWalkable ? highlightColor : defaultColor;
        siSR.enabled = isMouseOver;
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
    }

    // void OnMouseEnter()
    // {
    //     // transform.localScale += Vector3.one * hoverAmount;
    //     // spriteRenderer.color = hoverColor;
    //     isMouseOver = true;
    // }

    void OnMouseExit()
    {
        // transform.localScale = defaultScale;
        // spriteRenderer.color = isWalkable ? highlightColor : defaultColor;
        isMouseOver = false;
    }

    public bool IsRoadTile() => isRoadTile;

    public bool IsClear()
    {
        return !Physics2D.OverlapCircle(transform.position, isClearRadius, obstacleLayer);
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

    // draw a gizmo matching the OverlapCircle of IsClear
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, isClearRadius);
    }
}
