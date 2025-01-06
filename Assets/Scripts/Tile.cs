using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;

    [SerializeField] Color hoverColor;
    [SerializeField] Color highlightColor = Color.yellow;

    [Header("Other Layers")]
    [SerializeField] LayerMask occupyingLayers;
    [SerializeField] LayerMask roadLayer;
    [SerializeField] LayerMask unitLayer;
    [SerializeField] float layerCheckRadius = 0.2f;

    private Color defaultColor;

    private SpriteRenderer sr;
    private Vector3 defaultScale;

    GameMaster gm;
    bool isWalkable = false;
    bool isMouseOver = false;
    bool isEdgeTile = false;
    bool isSpawnTile = false;
    int unitDistance = 0;

    public int GetUnitDistance() => unitDistance;

    public bool IsSpawnTile()
    {
        return isSpawnTile;
    }

    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        sr = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
        defaultColor = sr.color;
        // select a random sprite
        if (sprites.Length > 0)
        {
            sr.sprite = sprites[Random.Range(0, sprites.Length)];
        }
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
        if (gm == null) gm = FindObjectOfType<GameMaster>();
        if (gm == null) Debug.LogError("GameMaster not found");

        if (isWalkable) // highlighted
        {
            Unit selectedUnit = gm.GetSelectedUnit();
            if (selectedUnit != null)
            {
                selectedUnit.MoveToTile(this);
            }
        }
        else
        {
            if (gm.InModeUnitActive())
            {
                gm.DeselectUnit();
            }
            else if (gm.InModeIdle())
            {
                // 
                Debug.Log("Nothing to do here!");
            }
            else if (gm.InModePlaceUnit())
            {
                Debug.Log("Placing new unit on tile: " + gameObject.name);
                gm.OpenStore(transform.position);
            }
        }
    }

    public void GainFocus()
    {
        isSpawnTile = FindSpawnable();
    }

    public void LoseFocus()
    {
        isSpawnTile = false;
    }

    bool FindSpawnable()
    {
        if (!IsClear()) return false;
        if (isEdgeTile) return true;
        // if(!(gm.InIdleMode() || gm.InPlaceNewUnitMode())) return false;
        // Debug.Log("FindSpawnable: GIM = " + gm.GameInputMode);

        bool foundSpawnableUnit = false;
        // use OverlapCircle to get a list of Units within a 2 unit radius of this tile
        Collider2D[] unitColliders = Physics2D.OverlapCircleAll(transform.position, 2.0f, unitLayer);
        // test the Unit of each collider
        Unit testUnit = null;
        for (int i = 0; i < unitColliders.Length; i++)
        {
            testUnit = unitColliders[i].GetComponent<Unit>();
            if (testUnit != null)
            {
                if (testUnit.BelongsToPlayer())
                {
                    if (testUnit.SpawnRadius > 0)
                    {
                        float distanceX = Mathf.Abs(testUnit.transform.position.x - transform.position.x);
                        float distanceY = Mathf.Abs(testUnit.transform.position.y - transform.position.y);
                        float unitDistance = distanceX + distanceY;
                        if (unitDistance <= testUnit.SpawnRadius)
                        {
                            foundSpawnableUnit = true;
                            Debug.Log("Spawnable tile: " + gameObject.name);
                            break;
                        }
                    }
                }
            }
        }
        return foundSpawnableUnit;
    }

    public bool HasRoad()
    {
        return Physics2D.OverlapCircle(transform.position, layerCheckRadius, roadLayer);
    }

    public bool IsClear() => IsClear(occupyingLayers);

    public bool IsClear(LayerMask layerMask)
    {
        return !Physics2D.OverlapCircle(transform.position, layerCheckRadius, layerMask);
    }

    public void Highlight(int distance)
    {
        // Debug.Log("Highlight tile: " + gameObject.name);
        isWalkable = true;
        unitDistance = distance;
    }

    public void ResetTile()
    {
        // Debug.Log("Reset tile: " + gameObject.name);
        isWalkable = false;
        unitDistance = 0;
    }

    // draw a gizmo matching the OverlapCircle of layerCheckRadius
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, layerCheckRadius);
    }
}
