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

    void Start()
    {
        units = FindObjectsOfType<Unit>();
        tiles = FindObjectsOfType<Tile>();

        LoadPlayers();
        SetCurrentPlayer(1);
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
                Debug.LogError("Player with ID 'None' found! " + player.name);
            }
            else if (players.ContainsKey(playerID))
            {
                Debug.LogError("Player with ID " + playerID + " already exists in the dictionary!");
            }
            else
            {
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
    public Player GetPlayer(PlayerID playerID) => players[playerID];
    public Player GetPlayer(int playerNumber) => players[(playerNumber == 1) ? PlayerID.Player1 : PlayerID.Player2];

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
            tile.Reset();
            tile.SetMouseOver(false);
        }
    }

    void ResetUnits()
    {
        foreach (Unit unit in units)
        {
            unit.Reset();
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
