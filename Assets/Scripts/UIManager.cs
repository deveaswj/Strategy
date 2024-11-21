using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;    
using TMPro;

public class UIManager : MonoBehaviour
{
    private Dictionary<PlayerID, PlayerUI> playerUIs = new();

    void Awake()
    {
        // find all player UIs in the scene and add them to the dictionary
        PlayerUI[] puis = FindObjectsOfType<PlayerUI>();
        foreach (PlayerUI playerUI in puis)
        {
            PlayerID playerID = playerUI.PlayerID;
            if (playerUIs.ContainsKey(playerID))
            {
                Debug.LogError("PlayerUI with ID " + playerID + " already exists in the dictionary!");
            }
            else
            {
                playerUIs.Add(playerID, playerUI);
            }
        }
    }

    void OnEnable()
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            player.OnHealthChanged += SetHealth;
            player.OnCurrencyChanged += SetCurrency;
        }
    }

    void OnDisable()
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            player.OnHealthChanged -= SetHealth;
            player.OnCurrencyChanged -= SetCurrency;
        }
    }

    public PlayerUI GetPlayerUI(PlayerID id)
    {
        if (!playerUIs.TryGetValue(id, out PlayerUI ui))
        {
            Debug.LogWarning("PlayerUI with ID " + id + " not found in the dictionary!");
            return null;
        }
        return ui;
    }

    public void SetHealth(PlayerID playerID, int health)
    {
        Debug.Log("UI: SetHealth for " + playerID + " to " + health);
        PlayerUI playerUI = GetPlayerUI(playerID);
        if (playerUI == null) return;
        playerUI.SetHealth(health);
    }

    public void SetCurrency(PlayerID playerID, int currency)
    {
        Debug.Log("UI: SetCurrency for " + playerID + " to " + currency);
        PlayerUI playerUI = playerUIs[playerID];
        if (playerUI == null) return;
        playerUI.SetCurrency(currency);
    }

}
