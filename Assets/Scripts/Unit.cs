using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Unit : MonoBehaviour
{
    [SerializeField] PlayerID playerID;
    [SerializeField] UnitStats unitStats;

    [Header("UI")]
    [SerializeField] float hoverAmount = 1.1f;
    [SerializeField] Color highlightColor = Color.yellow;
    [SerializeField] float moveSpeed = 5f;

    [Header("Effects")]
    [SerializeField] DamageIcon damageIconPrefab;

    [Header("Other Layers")]
    [SerializeField] LayerMask roadLayer;
    [SerializeField] float layerCheckRadius = 0.2f;

    [SerializeField] int healthOverride = 0;
    [SerializeField] float spawnRadius = 0f;

    private int health;

    private List<Unit> enemiesInRange = new();

    private bool selected = false;
    private bool hasMoved = false;
    private bool movedLastTurn = false;
    private float attackableRange = 0;
    private bool hasAttacked = false;

    private GameObject attackIndicator;
    private SpriteRenderer aiSR;

    private Color defaultColor;

    private SpriteRenderer spriteRenderer;
    private Vector3 defaultScale;

    GameMaster gm;


    private ExplosionPoolManager explosionPoolManager;

    private List<Tile> traversableTiles;

    public PlayerID PlayerID => playerID;
    public float SpawnRadius => spawnRadius;
    public bool HasAttacked => hasAttacked;
    public bool IsAttackable() => (attackableRange > 0);
    public float AttackableRange => attackableRange;

    public bool BelongsToPlayer() => (playerID == gm.CurrentPlayer);
    public bool BelongsToPlayer(PlayerID id) => (playerID == id);

    protected void Awake()
    {
        traversableTiles = new();
        health = unitStats.maxHealth;
        if (healthOverride > 0) health = healthOverride;
        if (hoverAmount < 1) hoverAmount += 1;
        explosionPoolManager = FindObjectOfType<ExplosionPoolManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected void Start()
    {
        gm = FindObjectOfType<GameMaster>();
        defaultScale = transform.localScale;
        defaultColor = spriteRenderer.color;
        attackIndicator = transform.Find("AttackIndicator").gameObject;
        aiSR = attackIndicator.GetComponent<SpriteRenderer>();
        UpdateBossHealth();
    }

    protected void Update()
    {
        spriteRenderer.color = selected ? highlightColor : defaultColor;
        aiSR.enabled = IsAttackable();
    }

    public void Select()
    {
        selected = true;
    }

    public void Deselect()
    {
        ClearEnemies();
        ClearTraversableTiles();
        selected = false;
    }

    void OnDisable()
    {
        Deselect();
    }

    public void ResetUnit(PlayerID currentPlayer)
    {
        ClearTraversableTiles();
        if (playerID == currentPlayer)
        {
            // save the last hasMoved state
            movedLastTurn = hasMoved;
        }
        hasMoved = false;
        hasAttacked = false;
        selected = false;
        ClearAttackable();
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
                GetTraversableTiles();
            }
        }
        else // this unit is enemy
        {
            if (IsAttackable())
            {
                // selected unit attacks if it can
                Unit selectedUnit = gm.GetSelectedUnit();
                if (selectedUnit != null && !selectedUnit.HasAttacked)
                {
                    float range = AttackableRange;
                    selectedUnit.Attack(this, range);
                    CounterAttack(selectedUnit, range);
                    // gm.AttackUnit(this);
                }
            }
        }
    }

    public void MoveTo(Vector2 position)
    {
        ClearTraversableTiles();
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

    public void GainFocus()
    {
        if (playerID == gm.CurrentPlayer || IsAttackable())
        {
            transform.localScale *= hoverAmount;
        }
    }

    public void LoseFocus()
    {
        transform.localScale = defaultScale;
    }

    void ClearTraversableTiles()
    {
        foreach (Tile tile in traversableTiles)
        {
            tile.ResetTile();
        }
        traversableTiles.Clear();
    }

    public void GetTraversableTiles()
    {
        if (hasMoved) return;

        bool isOnRoad = IsOnRoad();
        int roadsBonus = unitStats.roadsBonus;

        bool tileIsClear;
        float distanceX, distanceY, unitDistance;

        traversableTiles.Clear();
        foreach (Tile tile in gm.GetTiles())
        {
            tileIsClear = tile.IsClear();
            distanceX = Mathf.Abs(tile.transform.position.x - transform.position.x);
            distanceY = Mathf.Abs(tile.transform.position.y - transform.position.y);
            unitDistance = distanceX + distanceY;

            if (unitDistance <= unitStats.travelRange)
            {
                if (tileIsClear)
                {
                    tile.Highlight();
                    traversableTiles.Add(tile);
                }
            }
            // calculate +1 bonus movement if eligible
            else if (roadsBonus > 0 && unitDistance <= (unitStats.travelRange + roadsBonus))
            {
                if (isOnRoad && tileIsClear && tile.HasRoad())
                {
                    tile.Highlight();
                    traversableTiles.Add(tile);
                }
            }
        }
    }

    void ClearEnemies()
    {
        foreach (Unit unit in enemiesInRange)
        {
            unit.ClearAttackable();
        }
        enemiesInRange.Clear();
    }

    void GetEnemiesInRange()
    {
        enemiesInRange.Clear();
        if (hasAttacked) return;

        float distanceX, distanceY, unitDistance;

        foreach (Unit unit in gm.GetEnemyUnits())
        {
            if (unit == null) continue;
            if (unit.playerID != playerID)
            {
                distanceX = Mathf.Abs(unit.transform.position.x - transform.position.x);
                distanceY = Mathf.Abs(unit.transform.position.y - transform.position.y);
                unitDistance = distanceX + distanceY;

                if (unitDistance <= unitStats.attackRange)
                {
                    unit.SetAttackable(unitDistance);
                    enemiesInRange.Add(unit);
                }
            }
        }
    }

    public void SetAttackable(float distance)
    {
        attackableRange = distance;
    }

    public void ClearAttackable()
    {
        attackableRange = 0;
    }

    public void Attack(Unit target, float range)
    {
        if (target.IsAttackable())
        {
            // check target's AttackableRange to determine damage
            int attackDamage = unitStats.GetAttackDamageByDistance(range);
            Debug.Log(gameObject.name + " attacks " + target.name + " for " + attackDamage + " ...");
            target.TakeDamage(attackDamage);
            hasAttacked = true;
            // we can't attack again this turn
            // #NOTE: #BUG - This clears out AttackableRange before the Counterattack (which relies on AttackableRange) can happen
            // #NOTE: Trying to fix bug by passing in range instead (maybe it'll work)
            ClearEnemies();
        }
        else
        {
            Debug.Log(gameObject.name + " can't attack " + target.name + "!");
        }
    }

    public void CounterAttack(Unit target, float range)
    {
        if (health <= 0)
        {
            Debug.Log("CounterAttack: Unit " + gameObject.name + " is dead!");
            return;
        }
        // check our AttackableRange to determine damage
        int counterDamage = unitStats.GetAttackDamageByDistance(range, countering: true);
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
        if (armorValue > 0)
        {
            armorValue--;
            Debug.Log(gameObject.name + " has " + armorValue + " armor left!");
        }

        if (health <= 0)
        {
            if (unitStats.isBoss)
            {
                gm.GameOver();
            }
            Die();
        }
    }

    public bool IsOnRoad()
    {
        return Physics2D.OverlapCircle(transform.position, layerCheckRadius, roadLayer);
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

    public void SetPlayerID(PlayerID newPlayerID)
    {
        playerID = newPlayerID;
        Debug.Log("Player ID: " + playerID);
        // set the icon according to unitStats
        spriteRenderer.sprite = unitStats.GetSpriteForPlayer(playerID);
        // Player2 units are flipped
        spriteRenderer.flipX = (playerID == PlayerID.Player2);
    }

    public void SetUnitStats(UnitStats newUnitStats) => unitStats = newUnitStats;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (spawnRadius > 0)
        {
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
