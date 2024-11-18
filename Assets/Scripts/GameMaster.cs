using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerID { None, Player1, Player2 };

public class GameMaster : MonoBehaviour
{
    [SerializeField] GameObject selectedUnitSquare;
    private Unit selectedUnit;

    public PlayerID CurrentPlayer { get { return currentPlayer; } }
    private PlayerID currentPlayer = PlayerID.Player1;
    private int playerTurn = 1;

    private Unit[] units;
    private Tile[] tiles;

    void Start()
    {
        units = FindObjectsOfType<Unit>();
        tiles = FindObjectsOfType<Tile>();
        SetCurrentPlayer(1);
    }

    public Unit[] GetUnits() => units;
    public Tile[] GetTiles() => tiles;

    void Update()
    {
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

    public void EndTurn()
    {
        DeselectUnit();
        ResetUnits();
        ResetTiles();
        SwitchPlayer();
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
}
