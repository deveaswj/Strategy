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

    [Header("Stats")]
    [SerializeField] int maxHealth = 5;
    [SerializeField] int attackDamage = 1;  // damage to deal when initiating attack
    [SerializeField] int counterDamage = 1;  // damage to deal when counter-attacking
    [SerializeField] int armorValue = 0;    // damage reduction when defending

    [Header("Effects")]
    [SerializeField] DamageIcon damageIconPrefab;

    private int health;

    private List<Unit> enemiesInRange = new();

    [Header("Public")]
    public bool hasMoved = false;
    public bool hasAttacked = false;
    public bool selected = false;
    public bool attackable = false;

    private GameObject attackIndicator;
    private SpriteRenderer aiSR;

    private Color defaultColor;

    private SpriteRenderer spriteRenderer;
    private Vector3 defaultScale;

    GameMaster gm;

    private List<Tile> walkableTiles;


    void Awake()
    {
        walkableTiles = new();
        gm = FindObjectOfType<GameMaster>();
        health = maxHealth;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
        defaultColor = spriteRenderer.color;
        attackIndicator = transform.Find("AttackIndicator").gameObject;
        aiSR = attackIndicator.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.color = selected ? highlightColor : defaultColor;
        aiSR.enabled = attackable;
    }

    public void Reset()
    {
        ClearWalkableTiles();
        hasMoved = false;
        hasAttacked = false;
        selected = false;
        attackable = false;
    }

    public void HandleMouseClick()
    {
        if (gm == null) gm = FindObjectOfType<GameMaster>();
        if (gm == null) Debug.LogError("GameMaster not found");

        ClearEnemies();

        if (playerID == gm.CurrentPlayer)
        {
            // this unit is friendly
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
        else
        {
            // this unit is enemy
            if (attackable)
            {
                // attack if we can
                Unit selectedUnit = gm.GetSelectedUnit();
                if (selectedUnit == null) return;
                if (selectedUnit.hasAttacked) return;
                gm.AttackUnit(this);
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
        ClearEnemies();
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
        ResetAttackables();
        enemiesInRange.Clear();
    }

    void ResetAttackables()
    {
        foreach (Unit unit in enemiesInRange)
        {
            unit.attackable = false;
        }
    }

    void GetEnemiesInRange()
    {
        enemiesInRange.Clear();
        if (hasAttacked) return;

        foreach (Unit unit in gm.GetUnits())
        {
            if (unit == null) continue;
            if (unit.playerID != playerID)
            {
                float distanceX = Mathf.Abs(unit.transform.position.x - transform.position.x);
                float distanceY = Mathf.Abs(unit.transform.position.y - transform.position.y);
                if (distanceX + distanceY <= attackRange)
                {
                    enemiesInRange.Add(unit);
                    unit.attackable = true;
                }
            }
        }
    }

    public void Attack(Unit target)
    {
        if (target.attackable)
        {
            Debug.Log(gameObject.name + " attacks " + target.name + " ...");
            target.TakeDamage(attackDamage);
            hasAttacked = true;
        }
        else
        {
            Debug.Log(gameObject.name + " can't attack " + target.name + "!");
        }
    }

    public void CounterAttack(Unit target)
    {
        Debug.Log(gameObject.name + " counters " + target.name + " ...");
        target.TakeDamage(counterDamage);
    }

    void TakeDamage(int damage = 0)
    {
        damage = Mathf.Max(damage - armorValue, 0);
        health -= damage;
        Debug.Log("... " + gameObject.name + " takes " + damage + " damage!");
        ShowDamage(damage);

        // every hit takes out 1 armor
        if (armorValue > 0) armorValue--;
        Debug.Log(gameObject.name + " has " + armorValue + " armor left!");

        if (health <= 0)
        {
            Die();
        }
    }

    void ShowDamage(int damage)
    {
        DamageIcon dmg = Instantiate(damageIconPrefab, transform.position, Quaternion.identity);
        dmg.ShowDamage(damage);
    }

    void Die()
    {
        Debug.Log(gameObject.name + " dies!");
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

}
