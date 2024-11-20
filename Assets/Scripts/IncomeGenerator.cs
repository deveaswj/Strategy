using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomeGenerator : MonoBehaviour
{
    [SerializeField] private int incomePerTurn = 0;
    private PlayerID playerID;

    public int GetIncome()
    {
        return incomePerTurn;
    }
    public PlayerID PlayerID => playerID;

    void Start()
    {
        Unit unit = GetComponent<Unit>();
        playerID = unit.PlayerID;
    }
}
