using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] PlayerID playerID;
    [SerializeField] float hoverAmount = 0.1f;
    [SerializeField] Color highlightColor = Color.yellow;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] int travelRange = 1;     // How many tiles the unit moves per turn

    [SerializeField] int attackRange = 1;

    private List<Unit> enemiesInRange = new();

    public bool hasMoved;
    public bool hasAttacked;
    public bool selected;

    private GameObject attackIndicator;
    private SpriteRenderer aiSR;
    private bool attackable = false;

    private Color defaultColor;

    private SpriteRenderer spriteRenderer;
    private Vector3 defaultScale;

    GameMaster gm;

    private List<Tile> walkableTiles = new List<Tile>();


    void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
        defaultColor = spriteRenderer.color;
        attackIndicator = transform.Find("AttackIndicator").gameObject;
        aiSR = attackIndicator.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.color = selected ? highlightColor : defaultColor;
    }

    public void HandleMouseClick()
    {
        if (gm == null) gm = FindObjectOfType<GameMaster>();
        if (gm == null) Debug.LogError("GameMaster not found");

        if (selected)
        {
            selected = false;
            gm.DeselectUnit();
        }
        else
        {
            if (playerID == gm.CurrentPlayer)
            {
                gm.DeselectUnit();
                selected = true;
                gm.SelectUnit(this);
                GetEnemiesInRange();
                GetWalkableTiles();
            }
        }
    }

    public void MoveTo(Vector2 position)
    {
        ClearWalkableTiles();
        StartCoroutine(StartMovement(position));
        // transform.position = position;
        // hasMoved = true;
    }

    public void MoveToTile(Tile tile)
    {
        Vector2 tilePosition = tile.transform.position;
        MoveTo(tilePosition);
    }

    IEnumerator StartMovement(Vector2 targetPosition)
    {
        while (transform.position.x != targetPosition.x)
        {
            Vector2 targetX = new(targetPosition.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetX, moveSpeed * Time.deltaTime);
            yield return null;
        }

        while (transform.position.y != targetPosition.y)
        {
            Vector2 targetY = new(transform.position.x, targetPosition.y);
            transform.position = Vector2.MoveTowards(transform.position, targetY, moveSpeed * Time.deltaTime);
            yield return null;
        }
        hasMoved = true;
        GetEnemiesInRange();
    }

    void OnMouseEnter()
    {
        transform.localScale += Vector3.one * hoverAmount;
    }

    void OnMouseExit()
    {
        transform.localScale = defaultScale;
    }

    public void ClearWalkableTiles()
    {
        foreach (Tile tile in walkableTiles)
        {
            tile.Reset();
        }
        walkableTiles.Clear();
    }

    public void GetWalkableTiles()
    {
        if (hasMoved) return;

        walkableTiles.Clear();
        foreach (Tile tile in FindObjectsOfType<Tile>())
        {
            float distanceX = Mathf.Abs(tile.transform.position.x - transform.position.x);
            float distanceY = Mathf.Abs(tile.transform.position.y - transform.position.y);
            if (distanceX + distanceY <= travelRange)
            {
                if (tile.IsClear())
                {
                    tile.Highlight();
                    walkableTiles.Add(tile);
                }
            }
        }
    }

    void ClearEnemies()
    {
        foreach (Unit unit in enemiesInRange)
        {
                // #TODO
        }
    }

    void GetEnemiesInRange()
    {
        enemiesInRange.Clear();
        if (hasAttacked) return;

        foreach (Unit unit in gm.GetUnits())
        {
            if (unit.playerID != playerID)
            {
                float distanceX = Mathf.Abs(unit.transform.position.x - transform.position.x);
                float distanceY = Mathf.Abs(unit.transform.position.y - transform.position.y);
                if (distanceX + distanceY <= attackRange)
                {
                    enemiesInRange.Add(unit);
                }
            }
        }
    }
}
