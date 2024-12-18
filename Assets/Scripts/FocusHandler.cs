using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Attach this to the selection (focus) indicator
// Unified handling of mouse and keyboard control of the selection (focus) indicator
// At any time, we'll want to know which unit and tile (if any) have the focus.
// The mouse can change the focus by moving around -- focus moves to the tile/unit under the mouse pointer.
// The keyboard can change the focus by WASD/Arrows.
// When keyboard has control of the focus, the mouse can't move the focus.
// If the user uses the keyboard to move the indicator, the mouse stays put (we can't move the mouse with the keyboard)
// So if the user clicks where the focus isn't, ignore the click.

public class FocusHandler : MonoBehaviour
{
    [SerializeField] LayerMask unitLayer;
    [SerializeField] LayerMask tileLayer;
    [SerializeField] GameMaster gm;
    [SerializeField] Sprite selectionSprite;
    [SerializeField] Color selectionColor = Color.white;
    [SerializeField] Sprite wrenchSprite;
    [SerializeField] Color wrenchColor = Color.yellow;

    Tile focusTile = null;
    Unit focusUnit = null;

    public Tile FocusTile { get => focusTile; }
    public Unit FocusUnit { get => focusUnit; }

    bool mouseHasFocus = true;

    Vector2 lastMousePosition = Vector2.zero;

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;
    }

    // Keyboard move handling
    public void OnMove(InputAction.CallbackContext context)
    {
        // Debug.Log(context.phase);
        if (context.performed)
        {
            // Get the raw input direction
            Vector2 direction = context.ReadValue<Vector2>();

            // Normalize to ensure the result is either cardinal or diagonal
            float x = Mathf.RoundToInt(direction.x);
            float y = Mathf.RoundToInt(direction.y);
            Vector2 snappedDirection = new(x, y);

            // Ensure only cardinal directions are processed
            if (x != 0 && y != 0)
            {
                Debug.Log("FH: OnMove: Ignoring diagonal input: " + snappedDirection);
            }
            else
            {
                Debug.Log("FH: OnMove: Direction = " + direction + ", Snapped = " + snappedDirection);

                if (TryMove(snappedDirection))
                {
                    // update focusTile and focusUnit
                    SetFocusTile(GetTile(transform.position));
                    SetFocusUnit(GetUnit(transform.position));
                    // keyboard now has focus control
                    // mouse may or may not be within the focus tile
                    // if it is, that'll get resolved in Update()
                    Debug.Log("FH: OnMove: Keyboard has focus");
                    mouseHasFocus = false;
                }
            }
        }
    }

    // Mouse click / keyboard space handling
    public void OnFire(InputAction.CallbackContext context)
    {
        string controlPath = context.control.path;
        Debug.Log("FH: OnFire: Control Path: " + controlPath);
        if (controlPath.StartsWith("/Mouse") && !mouseHasFocus)
        {
            Debug.Log("FH: OnFire: Mouse but not focused - return");
            return;
        }

        if (context.performed)
        {
            Debug.Log("FH: OnFire: Context Performed");
            // if we have a unit, handle its mouse click
            if (focusUnit != null)
            {
                focusUnit.HandleMouseClick();
            }
            // else, if we have a tile, handle its mouse click
            else if (focusTile != null)
            {
                focusTile.HandleMouseClick();
            }
        }
    }

    // Mouse move handling
    // (Mouse click moved to OnFire())
    private void Update()
    {
        if (!gm.IsMousable()) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mousePosition != lastMousePosition)
        {
            lastMousePosition = mousePosition;

            Tile tile = GetTile(mousePosition);
            Unit unit = GetUnit(mousePosition);

            if (!mouseHasFocus && tile != null && tile == focusTile)
            {
                // mouse is inside the focus tile
                // bring focus control back to the mouse
                Debug.Log("FH: Update: Mouse has focus");
                mouseHasFocus = true;
            }

            if (mouseHasFocus)
            {
                // Tile hover effect
                if (tile != null)
                {
                    if (tile != focusTile)
                    {
                        // Debug.Log("FH: Update: Tile hover / assign new Focus objects");

                        MoveTo(tile.transform.position);
                        SetFocusTile(tile);
                        
                        unit = GetUnit(tile.transform.position);
                        SetFocusUnit(unit);
                    }
                }
            }
        }

        // which sprite do we display?
        // if idle and tile is spawnable, show wrench sprite
        // else, show selection sprite
        bool wrenchable = (gm.InIdleMode() || gm.InPlaceMode());
        // Debug.Log("Wrenchable? " + wrenchable + ", GameInputMode: " + gm.GameInputMode);
        if (wrenchable && focusTile != null && focusTile.IsSpawnTile())
        {
            spriteRenderer.sprite = wrenchSprite;
            spriteRenderer.color = wrenchColor;
            gm.SetPlaceMode();
        }
        else
        {
            spriteRenderer.sprite = selectionSprite;
            spriteRenderer.color = selectionColor;
            if (gm.InPlaceMode())
            {
                gm.SetIdleMode();
            }
        }
    }

    public bool TryMove(Vector2 direction)
    {
        bool success = false;
        Vector3 targetPosition = OffsetPosition(direction);
        if (Physics2D.OverlapCircle(targetPosition, 0.1f, tileLayer) != null)
        {
            MoveTo(targetPosition);
            success = true;
        }
        return success;
    }

    public void MoveTo(Vector2 position)
    {
        // move this object to the given position
        // Debug.Log("FH: Moving selection indicator to " + position);
        transform.position = position;
    }

    public Vector3 OffsetPosition(Vector3 offset)
    {
        return transform.position + offset;
    }

    public Vector3 OffsetPosition(Vector2 offset) => OffsetPosition(new Vector3(offset.x, offset.y, 0));
    public Vector3 OffsetPosition(float x, float y, float z = 0) => OffsetPosition(new Vector3(x, y, z));

    private Unit GetUnit(Vector2 atPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(atPosition, Vector2.zero, Mathf.Infinity, unitLayer);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Unit>();
        }
        return null;
    }

    private Tile GetTile(Vector2 atPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(atPosition, Vector2.zero, Mathf.Infinity, tileLayer);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

    void SetFocusTile(Tile newTile)
    {
        focusTile?.LoseFocus();
        newTile?.GainFocus();
        focusTile = newTile;
    }

    void SetFocusUnit(Unit newUnit)
    {
        // string newUnitNull = "newUnit is" + (newUnit == null ? "" : "not") + " null";
        // string focusUnitNull = "focusUnit is" + (focusUnit == null ? "" : "not") + " null";
        // Debug.Log("FH: SetFocusUnit: " + newUnitNull + ", " + focusUnitNull);

        focusUnit?.LoseFocus();
        newUnit?.GainFocus();
        focusUnit = newUnit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
        Gizmos.DrawWireSphere(transform.position, 2.0f);
    }

}
