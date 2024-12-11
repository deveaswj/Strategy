using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UnitTravelType { None, RoadOnly, RoadPrefer, Offroad };
// RoadOnly: can only move on roads
// RoadPrefer: Movement penalty when offroad
// Offroad: No movement penalty

public class Unit : MonoBehaviour
{
    [SerializeField] PlayerID playerID;
    [SerializeField] UnitStats unitStats;

    // [Header("Stats")]
    // [SerializeField] int maxHealth = 5;
    // [SerializeField] int travelRange = 1;     // How many tiles the unit moves per turn
    // [SerializeField] UnitTravelType travelType = UnitTravelType.RoadPrefer;
    // [SerializeField] int attackRange = 1;
    // [Tooltip("Damage when attacking from 1 tile away")]
    // [SerializeField] int meleeDamage = 1;   // damage to deal when initiating melee attack
    // [Tooltip("Damage when attacking from 2+ tiles away")]
    // [SerializeField] int rangedDamage = 1;  // damage to deal when initiating ranged attack
    // [SerializeField] int counterRange = 1;  // halve counter-damage if outside this range
    // [SerializeField] int armorValue = 0;    // damage reduction when defending

    [Header("UI")]
    [SerializeField] float hoverAmount = 1.1f;
    [SerializeField] Color highlightColor = Color.yellow;
    [SerializeField] float moveSpeed = 5f;

    [Header("Effects")]
    [SerializeField] DamageIcon damageIconPrefab;

    // [Header("Boss")]
    // [SerializeField] bool isBoss = false;

    [SerializeField] int healthOverride = 0;

    private int health;

    private List<Unit> enemiesInRange = new();

    private bool selected = false;

    [Header("Public")]
    public bool hasMoved = false;
    public bool hasAttacked = false;
    public bool attackable = false;
    public float attackableRange = 0;

    private GameObject attackIndicator;
    private SpriteRenderer aiSR;

    private Color defaultColor;

    private SpriteRenderer spriteRenderer;
    private Vector3 defaultScale;

    GameMaster gm;
    public PlayerID PlayerID => playerID;

    private ExplosionPoolManager explosionPoolManager;

    private List<Tile> walkableTiles;

    protected void Awake()
    {
        walkableTiles = new();
        gm = FindObjectOfType<GameMaster>();
        health = unitStats.maxHealth;
        if (healthOverride > 0) health = healthOverride;
        if (hoverAmount < 1) hoverAmount += 1;
        explosionPoolManager = FindObjectOfType<ExplosionPoolManager>();
    }

    protected void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultScale = transform.localScale;
        defaultColor = spriteRenderer.color;
        attackIndicator = transform.Find("AttackIndicator").gameObject;
        aiSR = attackIndicator.GetComponent<SpriteRenderer>();
        UpdateBossHealth();
    }

    protected void Update()
    {
        spriteRenderer.color = selected ? highlightColor : defaultColor;
        aiSR.enabled = attackable;
    }

    public void Select()
    {
        selected = true;
    }

    public void Deselect()
    {
        ClearEnemies();
        ClearWalkableTiles();
        selected = false;
    }

    void OnDisable()
    {
        Deselect();
    }

    public void ResetUnit()
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

        if (playerID == gm.CurrentPlayer) // this unit is friendly
        {
            if (selected)
            {
                // deselect this unit (return to idle state)
                gm.DeselectUnit();
            }
            else
            {
                // deselect any other selected unit, and select this one
                gm.DeselectUnit();
                gm.SelectUnit(this);
                GetEnemiesInRange();
                GetWalkableTiles();
            }
        }
        else // this unit is enemy
        {
            if (attackable)
            {
                // selected unit attacks if it can
                Unit selectedUnit = gm.GetSelectedUnit();
                if (selectedUnit != null && !selectedUnit.hasAttacked)
                {
                    selectedUnit.Attack(this);
                    CounterAttack(selectedUnit);
                    // gm.AttackUnit(this);
                }
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

    // protected void OnMouseEnter()
    // {
    //     if (!gm.IsMousable()) return;
    //     GainFocus();
    // }

    // protected void OnMouseExit()
    // {
    //     LoseFocus();
    // }

    public void GainFocus()
    {
        if (playerID == gm.CurrentPlayer || attackable)
        {
            transform.localScale *= hoverAmount;
        }
    }

    public void LoseFocus()
    {
        transform.localScale = defaultScale;
    }

    void ClearWalkableTiles()
    {
        foreach (Tile tile in walkableTiles)
        {
            tile.ResetTile();
        }
        walkableTiles.Clear();
    }

    public void GetWalkableTiles()
    {
        if (hasMoved) return;

        walkableTiles.Clear();
        foreach (Tile tile in gm.GetTiles())
        {
            float distanceX = Mathf.Abs(tile.transform.position.x - transform.position.x);
            float distanceY = Mathf.Abs(tile.transform.position.y - transform.position.y);
            if (distanceX + distanceY <= unitStats.travelRange)
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
            unit.attackableRange = 0;
        }
    }

    void GetEnemiesInRange()
    {
        enemiesInRange.Clear();
        if (hasAttacked) return;

        foreach (Unit unit in gm.GetEnemyUnits())
        {
            if (unit == null) continue;
            if (unit.playerID != playerID)
            {
                float distanceX = Mathf.Abs(unit.transform.position.x - transform.position.x);
                float distanceY = Mathf.Abs(unit.transform.position.y - transform.position.y);
                float unitDistance = distanceX + distanceY;
                if (unitDistance <= unitStats.attackRange)
                {
                    enemiesInRange.Add(unit);
                    unit.attackable = true;
                    unit.attackableRange = unitDistance;
                }
            }
        }
    }

    public int GetAttackDamageByDistance(float distance)
    {
        // return melee if distance is 1
        if (distance == 1) return unitStats.meleeDamage;
        // if distance is greater than counterRange, return half rangedDamage (or 1 at minimum)
        // else return full rangedDamage
        if (distance > unitStats.counterRange)
        {
            // return half rangedDamage -- rounded, but at least 1
            return Mathf.Max(Mathf.RoundToInt(unitStats.rangedDamage * 0.5f), 1);
        }
        else
        {
            // return full rangedDamage
            return unitStats.rangedDamage;
        }
    }

    public void Attack(Unit target)
    {
        if (target.attackable)
        {
            // check target's attackableRange to determine damage
            int attackDamage = GetAttackDamageByDistance(target.attackableRange);
            Debug.Log(gameObject.name + " attacks " + target.name + " for " + attackDamage + " ...");
            target.TakeDamage(attackDamage);
            hasAttacked = true;
            // we can't attack again this turn
            ClearEnemies();
        }
        else
        {
            Debug.Log(gameObject.name + " can't attack " + target.name + "!");
        }
    }

    public void CounterAttack(Unit target)
    {
        if (health <= 0)
        {
            Debug.Log("CounterAttack: Unit " + gameObject.name + " is dead!");
            return;
        }
        // check our own attackableRange to determine damage
        int counterDamage = GetAttackDamageByDistance(attackableRange);
        Debug.Log(gameObject.name + " counters " + target.name + " for " + counterDamage  + " ...");
        target.TakeDamage(counterDamage);
    }

    void TakeDamage(int damage = 0)
    {
        int armorValue = unitStats.armorValue;
        damage = Mathf.Max(damage - armorValue, 0);
        health -= Mathf.Min(damage, health);    // don't take more damage than health
        Debug.Log("... " + gameObject.name + " takes " + damage + " damage!");
        ShowDamage(damage);
        UpdateBossHealth();

        // every hit takes out 1 armor, regardless of damage taken
        if (armorValue > 0) armorValue--;
        Debug.Log(gameObject.name + " has " + armorValue + " armor left!");

        if (health <= 0)
        {
            if (unitStats.isBoss)
            {
                gm.GameOver(playerID);
            }
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
        Explode();
        Debug.Log(gameObject.name + " dies!");
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    void Explode()
    {
        PlayExplosion explosion = explosionPoolManager.GetExplosion();
        explosion.transform.position = transform.position;
        explosion.Explode();
    }

    public void UpdateBossHealth()
    {
        if (unitStats.isBoss)
        {
            Debug.Log("Updating " + playerID + " boss health: " + health);
            gm.SetHealth(health, playerID);
        }
    }

}
