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

    public PlayerID PlayerID => playerID;
    public int Health => health;
    public int Currency => currency;

    public GameObject Units => unitsContainer;

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

    public void AddCurrency(int amount)
    {
        SetCurrency(currency + amount);
    }
}
