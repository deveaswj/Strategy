using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event Action<PlayerID, int> OnHealthChanged;
    public event Action<PlayerID, int> OnCurrencyChanged;

    [SerializeField] PlayerID playerID;
    [SerializeField] int health = 100;
    [SerializeField] int currency = 100;
    [SerializeField] GameObject unitsContainer;
    [SerializeField] GameObject centerTile;

    public PlayerID PlayerID => playerID;
    public int Health => health;
    public int Currency => currency;

    public GameObject Units => unitsContainer;

    public Unit[] GetUnits() => unitsContainer.GetComponentsInChildren<Unit>();

    public Unit[] GetUnitsInOrder(bool sortLeftToRight = true)
    {
        Unit[] units = GetUnits();
        // Unit[] units = unitsContainer.GetComponentsInChildren<Unit>();

        if (units == null || units.Length == 0)
            return new Unit[0]; // Handle empty or null arrays safely

        // Sort by y-position (descending for top-to-bottom), then x-position (variable based on sortLeftToRight)
        Array.Sort(units, (u1, u2) =>
        {
            if (u1 == null || u2 == null)
                return 0; // Handle potential null Units gracefully

            // Compare y-positions (descending order for top-to-bottom)
            int yComparison = u2.transform.position.y.CompareTo(u1.transform.position.y);
            if (yComparison != 0)
                return yComparison;

            // Compare x-positions (ascending or descending based on sortLeftToRight)
            return sortLeftToRight 
                ? u1.transform.position.x.CompareTo(u2.transform.position.x)   // Left-to-right
                : u2.transform.position.x.CompareTo(u1.transform.position.x); // Right-to-left
        });

        return units;
    }

    public Unit CreateUnit(UnitStats unitStats, Vector3 position)
    {
        GameObject newUnit = Instantiate(unitStats.unitPrefab, position, Quaternion.identity);
        newUnit.transform.parent = unitsContainer.transform;
        Unit unit = newUnit.GetComponent<Unit>();
        unit.SetPlayerID(playerID);
        unit.SetUnitStats(unitStats);
        return unit;
    }

    public Unit BuyUnit(UnitStats unitStats, Vector3 position)
    {
        Unit unit = CreateUnit(unitStats, position);
        AddCurrency(-unitStats.price);
        return unit;
    }

    public void SetHealth(int newHealth)
    {
        Debug.Log("Player " + playerID + " health: " + newHealth);
        health = newHealth;
        OnHealthChanged?.Invoke(playerID, health);
    }

    public void SetCurrency(int newCurrency)
    {
        Debug.Log("Player " + playerID + " currency: " + newCurrency);
        currency = newCurrency;
        OnCurrencyChanged?.Invoke(playerID, currency);
    }

    public int GetCurrency() => currency;

    public void AddCurrency(int amount)
    {
        SetCurrency(currency + amount);
    }
}
