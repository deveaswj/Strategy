using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerID { None, Player1, Player2 };

public class GameMaster : MonoBehaviour
{
    [SerializeField] GameObject selectedUnitSquare;
    private Unit selectedUnit;

    [SerializeField] TextMeshProUGUI turnText;

    public PlayerID CurrentPlayer { get { return currentPlayer; } }
    private PlayerID currentPlayer = PlayerID.Player1;
    private int playerTurn = 1;

    private Unit[] units;
    private Tile[] tiles;

    Dictionary<PlayerID, Player> players = new();

    void Awake()
    {
        LoadPlayers();
        SetCurrentPlayer(1);
    }

    void Start()
    {
        units = FindObjectsOfType<Unit>();
        tiles = FindObjectsOfType<Tile>();
    }

    public Unit[] GetUnits() => units;
    public Tile[] GetTiles() => tiles;

    void Update()
    {
        turnText.text = "Player " + playerTurn + "'s turn";
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }

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
        foreach (Unit unit in units)
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
            selectedUnit.ClearWalkableTiles();
            selectedUnit.selected = false;
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
