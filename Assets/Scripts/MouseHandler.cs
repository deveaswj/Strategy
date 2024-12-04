using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    [SerializeField] LayerMask unitLayer; // Set this to the Unit layer in the Inspector
    [SerializeField] LayerMask tileLayer; // Set this to the Tile layer in the Inspector
    [SerializeField] GameMaster gm;

    private void Update()
    {
        if (!gm.IsMousable()) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Unit unit = GetUnit(mousePosition);
        Tile tile = GetTile(mousePosition);
        bool foundUnit = (unit != null);
        bool foundTile = (tile != null);

        bool mouseClicked = Input.GetMouseButtonDown(0);

        // Tile hover effect
        if (foundTile)
        {
            tile.SetMouseOver(true);
        }

        // Unit click (overrides tile click)
        if (foundUnit)
        {
            if (mouseClicked)
            {
                unit.HandleMouseClick();
                return;
            }
        }

        // Tile click
        if (foundTile)
        {
            if (mouseClicked)
            {
                tile.HandleMouseClick();
            }
        }
    }

    private Unit GetUnit(Vector2 mousePosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, unitLayer);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Unit>();
        }
        return null;
    }

    private Tile GetTile(Vector2 mousePosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, tileLayer);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

}
