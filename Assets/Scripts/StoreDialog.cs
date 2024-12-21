using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class StoreDialog : MonoBehaviour
{
    public class StoreItem
    {
        public UnitStats unitStats;
        public bool enabled;
    }

    [SerializeField] GameObject dialogCanvas; // Reference to the Store dialog
    [SerializeField] MonoBehaviour focusHandler; // Reference to your FocusHandler script

    private PlayerInput playerInput; // PlayerInput component reference

    [SerializeField] UnitStats[] availableUnits;
    private int currentUnitIndex = 0;
    UnitStats userPurchase = null;
    StoreItem[] storeItems;

    Player customer;
    Vector3 targetPosition;

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI unitNameText;
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI statsText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button buyButton;
    [SerializeField] Button closeButton;

    InputActionMap playerInputMap;
    InputActionMap uiInputMap;

    void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        playerInputMap = playerInput.actions.FindActionMap("Player");
        uiInputMap = playerInput.actions.FindActionMap("UI");
        // Ensure the dialog starts disabled
        dialogCanvas.SetActive(false);
        uiInputMap.Disable();
    }

    void PopulateStoreUnits()
    {
        int budget = customer.GetCurrency();
        storeItems = new StoreItem[availableUnits.Length];
        for (int i = 0; i < availableUnits.Length; i++)
        {
            storeItems[i] = new StoreItem();
            UnitStats unitStats = availableUnits[i];
            storeItems[i].unitStats = unitStats;
            storeItems[i].enabled = (unitStats.price <= budget);
        }
    }


    public void OpenStore(Player player, Vector3 position)
    {
        customer = player;
        targetPosition = position;
        userPurchase = null;
        PopulateStoreUnits();
        FirstUnit();
        ShowDialog();
    }

    public void ShowDialog()
    {
        UpdateUnitDetails();
        dialogCanvas.SetActive(true);

        // Switch action maps: Disable Player and enable UI
        playerInputMap.Disable();
        uiInputMap.Enable();

        // Disable FocusHandler
        focusHandler.enabled = false;
    }

    public void CloseDialog()
    {
        dialogCanvas.SetActive(false);

        // Switch action maps: Disable UI and enable Player
        playerInputMap.Enable();
        uiInputMap.Disable();

        // Re-enable FocusHandler
        focusHandler.enabled = true;
    }

    public void BuyUnit()
    {
        userPurchase = storeItems[currentUnitIndex].unitStats;
        Debug.Log($"Buying {userPurchase.unitName}!");

        customer.BuyUnit(userPurchase, targetPosition);

        CloseDialog();
    }

    public void FirstUnit()
    {
        currentUnitIndex = 0;
        UpdateUnitDetails();
    }

    public void NextUnit()
    {
        currentUnitIndex = (currentUnitIndex + 1) % storeItems.Length;
        UpdateUnitDetails();
    }

    public void PreviousUnit()
    {
        currentUnitIndex = (currentUnitIndex - 1 + storeItems.Length) % storeItems.Length;
        UpdateUnitDetails();
    }

    private void UpdateUnitDetails()
    {
        UnitStats unitStats = storeItems[currentUnitIndex].unitStats;
        unitNameText.text = unitStats.unitName;
        iconImage.sprite = unitStats.iconGeneric;
        int onRoadMovement = unitStats.travelRange + unitStats.roadsBonus;
        statsText.text =
            "Price: " + unitStats.price + "\n\n" +
            "Hit Points: " + unitStats.maxHealth + "\n" +
            "Armor: " + unitStats.armorValue + "\n" +
            "Off-road Moves: " + unitStats.travelRange + "\n" +
            "On-road Moves: " + onRoadMovement + "\n" +
            "Melee Damage: " + unitStats.meleeDamage + "\n";

        if (unitStats.attackRange > 1)
        {
            statsText.text +=
                "Arms Damage: " + unitStats.rangedDamage + "\n" +
                "Arms Range: " + unitStats.attackRange;
        }

        descriptionText.text = unitStats.storeDescription;

        Debug.Log($"Unit: {unitStats.unitName}");
    }

    public void OnUICancel(InputAction.CallbackContext context)
    {
        CloseDialog();
    }

    public void OnUISubmit(InputAction.CallbackContext context)
    {
        BuyUnit();
    }

    public void OnUIScrollWheel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 scrollValue = context.ReadValue<Vector2>();
            float scrollAmount = scrollValue.y;
            if (scrollAmount < 0)
            {
                NextUnit();
            }
            else if (scrollAmount > 0)
            {
                PreviousUnit();
            }
        }
    }

}
