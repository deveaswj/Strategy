using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerID { None, Player1, Player2 };

public enum GameInputMode { None, Idle, UnitActive, BuyNewUnit, PlaceNewUnit, GameOver };
// None -- should never be used
// Idle -- select unit, buy new unit, or end turn
//      Buy Unit: Yes
//      Select:   Yes
//      End Turn: Yes
// UnitActive -- unit selected -- use it, select something else, deselect it, or end turn
//      Buy Unit: No
//      Select:   Yes
//      End Turn: Yes
// PlaceNewUnit -- place new unit -- cannot do anything else until done
//      Buy Unit: No
//      Select:   No
//      End Turn: No

public class GameMaster : MonoBehaviour
{
    [SerializeField] GameObject selectedUnitSquare;
    private Unit selectedUnit;

    [SerializeField] TextMeshProUGUI turnText;
    [SerializeField] GameObject centerTile;

    [SerializeField] LayerMask tileLayer; // Set this to the Tile layer in the Inspector
    [SerializeField] float tileSize = 64;

    public PlayerID CurrentPlayer { get { return currentPlayer; } }
    private PlayerID currentPlayer = PlayerID.Player1;
    private int playerTurn = 1;

    PlayerID winningPlayer = PlayerID.None;

    private Tile[] tiles;

    Dictionary<PlayerID, Player> players = new();

    [SerializeField] GameInputMode gameInputMode = GameInputMode.Idle;

    public GameInputMode GameInputMode { get { return gameInputMode; } }

    SelectionIndicator selectionIndicator;

    private StoreDialog storeDialog;

// public enum GameInputMode { None, Idle, UnitActive, BuyNewUnit, PlaceNewUnit, GameOver };
    bool InIdleMode() => (gameInputMode == GameInputMode.Idle);
    bool InUnitActiveMode() => (gameInputMode == GameInputMode.UnitActive);
    bool InBuyNewUnitMode() => (gameInputMode == GameInputMode.BuyNewUnit);
    bool InPlaceNewUnitMode() => (gameInputMode == GameInputMode.PlaceNewUnit);


    void Awake()
    {
        selectionIndicator = FindObjectOfType<SelectionIndicator>();
        storeDialog = FindObjectOfType<StoreDialog>();
        LoadPlayers();
        SetCurrentPlayer(1);
    }

    void Start()
    {
        tiles = FindObjectsOfType<Tile>();
        GrantIncome();
        // gameInputMode = GameInputMode.Idle;
        // gameInputMode = GameInputMode.BuyNewUnit;
    }

    public Unit[] GetUnits() => players[currentPlayer].GetUnits();
    public Unit[] GetUnits(PlayerID playerID) => players[playerID].GetUnits();

    // get units of all players that are not the current player
    public Unit[] GetEnemyUnits() => GetEnemyUnits(currentPlayer);
    public Unit[] GetEnemyUnits(PlayerID playerID)
    {
        PlayerID otherPlayer = (playerID == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;
        return players[otherPlayer].GetUnits();
    }

    public Tile[] GetTiles() => tiles;

    void Update()
    {
        turnText.text = "Player " + playerTurn + "'s turn";

        // if (gameInputMode == GameInputMode.Idle)
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         EndTurn();
        //     }
        // }

        UpdateSelectionUI();
    }

    public void GameOver(PlayerID playerID)
    {
        gameInputMode = GameInputMode.GameOver;
        winningPlayer = playerID;
        Debug.Log("Game Over! Winning player: " + winningPlayer);
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        // Pause for 1 second
        // then show the Game Over canvas
        yield return new WaitForSeconds(1.0f);
        Debug.Log("(Here's where we would show the Game Over canvas)");
    }

    public bool IsMousable()
    {
        return (gameInputMode == GameInputMode.Idle 
        || gameInputMode == GameInputMode.UnitActive
        || gameInputMode == GameInputMode.PlaceNewUnit);
    }


    void ShowStoreUI()
    {
        // gameInputMode = GameInputMode.BuyNewUnit;
        storeDialog.ShowDialog();
    }

    public void OnEndTurn()
    {
        // End Turn (if idle)
        if (gameInputMode == GameInputMode.Idle || gameInputMode == GameInputMode.UnitActive)
        {
            EndTurn();
        }
        if (gameInputMode == GameInputMode.PlaceNewUnit) {}
        if (gameInputMode == GameInputMode.BuyNewUnit) {}
    }

    public void OnEscape()
    {
        if (gameInputMode == GameInputMode.UnitActive)
        {
            DeselectUnit();
        }
        else if (gameInputMode == GameInputMode.PlaceNewUnit)
        {
            // CancelNewUnitPlacement();
        }
        else if (gameInputMode == GameInputMode.BuyNewUnit)
        {
            // 
        }
    }

    public void OnSelectNext()
    {
        // Select Next/Prev behavior:
        // If no unit is selected, select the active player's first unit
        // If a unit is selected, select the active player's next or previous unit
        string debugPrefix = "OnSelectNext (Player " + currentPlayer + "): ";
        if (selectedUnit == null)
        {
            // Select First
            Debug.Log(debugPrefix + "Select First");
        }
        else
            {
            bool shiftModifier = Keyboard.current.shiftKey.isPressed;
            if (shiftModifier)
            {
                // Select Previous
                Debug.Log(debugPrefix + "Select Previous");
            }
            else
            {
                // Select Next
                Debug.Log(debugPrefix + "Select Next");
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            Debug.Log("OnMove: " + direction);

            // Calculate the new position based on the direction vector
            Vector3 targetPosition = selectionIndicator.transform.position + new Vector3(direction.x, direction.y, 0) * tileSize;

            Debug.Log("Target Position: " + targetPosition);

            // Check if the target position contains a valid tile
            if (Physics2D.OverlapCircle(targetPosition, 0.1f, tileLayer) != null)
            {
                selectionIndicator.MoveTo(targetPosition);
                // selectionIndicator.transform.position = targetPosition;
            }
        }
    }

    void UpdateSelectionUI()
    {
        if (selectedUnit != null)
        {
            selectedUnitSquare.SetActive(true);
            selectedUnitSquare.transform.position = selectedUnit.transform.position;
        }
        else
        {
            selectedUnitSquare.SetActive(false);
        }
    }

    void LoadPlayers()
    {
        // find components of type Player and add them to the dictionary
        Player[] playerObjects = FindObjectsOfType<Player>();
        foreach (Player player in playerObjects)
        {
            PlayerID playerID = player.PlayerID;
            if (playerID == PlayerID.None)
            {
                Debug.LogError("LoadPlayers: Player with ID 'None' found! " + player.name);
            }
            else if (players.ContainsKey(playerID))
            {
                Debug.LogError("LoadPlayers: Player with ID " + playerID + " is already in the dictionary!");
            }
            else
            {
                Debug.Log("LoadPlayers: Adding player with ID " + playerID);
                players.Add(playerID, player);
            }
        }
    }

    public void SetCurrentPlayer(int playerNumber)
    {
        playerTurn = playerNumber;
        currentPlayer = (playerTurn == 1) ? PlayerID.Player1 : PlayerID.Player2;
    }

    public void SwitchPlayer()
    {
        int newPlayer = (playerTurn == 1) ? 2 : 1;
        SetCurrentPlayer(newPlayer);
    }

    public Player GetPlayer() => GetPlayer(currentPlayer);
    public Player GetPlayer(PlayerID playerID)
    {
        if (!players.TryGetValue(playerID, out Player player))
        {
            Debug.LogError("GetPlayer: Player with ID " + playerID + " not found in the dictionary!");
            return null;
        }
        Debug.Log("GetPlayer: Returning player with ID " + playerID);
        return player;
    }
    public Player GetPlayer(int playerNumber)
    {
        if (playerNumber == 1) return GetPlayer(PlayerID.Player1);
        if (playerNumber == 2) return GetPlayer(PlayerID.Player2);
        return null;
    }

    public void GrantIncome() => GrantIncome(currentPlayer);
    public void GrantIncome(PlayerID playerID)
    {
        // get the current player's units
        Player player = GetPlayer(playerID);
        GameObject playerUnits = player.Units;
        IncomeGenerator[] incomeGenerators = playerUnits.GetComponentsInChildren<IncomeGenerator>();

        int currencyThisTurn = 0;
        foreach (IncomeGenerator incomeGenerator in incomeGenerators)
        {
            currencyThisTurn += incomeGenerator.GetIncome();
        }
        player.AddCurrency(currencyThisTurn);
    }

    public void SetHealth(int health, PlayerID playerID) => GetPlayer(playerID).SetHealth(health);
    public void SetHealth(int health) => GetPlayer().SetHealth(health);
    public void SetCurrency(int currency, PlayerID playerID) => GetPlayer(playerID).SetCurrency(currency);
    public void SetCurrency(int currency) => GetPlayer().SetCurrency(currency);


    public void EndTurn()
    {
        DeselectUnit();
        ResetUnits();
        ResetTiles();
        SwitchPlayer();
        GrantIncome();
    }

    void ResetTiles()
    {
        foreach (Tile tile in tiles)
        {
            tile.ResetTile();
            tile.SetMouseOver(false);
        }
    }

    public void SetTileFocus(Transform tileTransform)
    {
        // move the SelectionIndicator to the tile
        selectionIndicator.MoveTo(tileTransform.position);
    }

    void ResetUnits()
    {
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            unit.ResetUnit();
        }
    }

    public void SelectUnit(Unit unit)
    {
        Debug.Log("Selecting unit: " + unit.name);
        selectedUnit = unit;
        selectedUnit.Select();
        gameInputMode = GameInputMode.UnitActive;
        Debug.Log("Game Input Mode: " + gameInputMode);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            Debug.Log("Deselecting unit: " + selectedUnit.name);
            selectedUnit.Deselect();
            selectedUnit = null;
            gameInputMode = GameInputMode.Idle;
            Debug.Log("Game Input Mode: " + gameInputMode);
        }
    }

    public void AttackUnit(Unit target)
    {
        if (selectedUnit != null)
        {
            if (target != null && target.attackable)
            {
                selectedUnit.Attack(target);

                if (target == null) return;     // null if target unit died
                target.CounterAttack(selectedUnit);
            }
        }
    }
}
