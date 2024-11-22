using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;    
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] PlayerID playerID;
    [SerializeField] Button menuButton;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI currencyText;

    public PlayerID PlayerID => playerID;

    public void SetHealth(int health) => healthText.text = $"HP: {health}";
    public void SetCurrency(int currency) => currencyText.text = $"CP: {currency}";
}
