using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerID { None, Player1, Player2 };

public enum GameInputMode { None, Idle, UnitActive, PlaceNewUnit, GameOver };
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

    public PlayerID CurrentPlayer { get { return currentPlayer; } }
    private PlayerID currentPlayer = PlayerID.Player1;
    private int playerTurn = 1;

    PlayerID winningPlayer = PlayerID.None;

    private Tile[] tiles;

    Dictionary<PlayerID, Player> players = new();

    GameInputMode gameInputMode = GameInputMode.None;

    void Awake()
    {
        LoadPlayers();
        SetCurrentPlayer(1);
    }

    void Start()
    {
        tiles = FindObjectsOfType<Tile>();
        GrantIncome();
        gameInputMode = GameInputMode.Idle;
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
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        // Pause for 1 second
        // then show the Game Over canvas
        yield return new WaitForSeconds(1.0f);

    }


    void OnSubmit()
    {
        // End Turn (if idle)
        if (gameInputMode == GameInputMode.Idle)
        {
            EndTurn();
        }
    }

    void OnCancel()
    {
        if (gameInputMode == GameInputMode.UnitActive)
        {
            DeselectUnit();
        }
        else if (gameInputMode == GameInputMode.PlaceNewUnit)
        {
            // CancelNewUnitPlacement();
        }
    }

    void OnSelectNext()
    {
        // Select Next/Prev behavior:
        // If no unit is selected, select the active player's first unit
        // If a unit is selected, select the active player's next or previous unit
        if (selectedUnit == null)
        {
            //
        }

        bool shiftModifier = Keyboard.current.shiftKey.isPressed;
        if (shiftModifier)
        {
            // Select Previous
            Debug.Log("Select Previous");
        }
        else
        {
            // Select Next
            Debug.Log("Select Next");
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
