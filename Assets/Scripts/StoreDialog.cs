using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class StoreDialog : MonoBehaviour
{
    [SerializeField] GameObject dialogCanvas; // Reference to the Store dialog
    [SerializeField] MonoBehaviour mouseHandler; // Reference to your MouseHandler script

    private PlayerInput playerInput; // PlayerInput component reference

    [SerializeField] UnitStats[] availableUnits;
    private int currentUnitIndex = 0;

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI unitNameText;
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI statsText;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button buyButton;
    [SerializeField] Button closeButton;

    void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        // Ensure the dialog starts disabled
        dialogCanvas.SetActive(false);
    }


    public void ShowDialog()
    {
        dialogCanvas.SetActive(true);

        // Switch action maps: Disable Player and enable UI
        playerInput.actions.FindActionMap("Player").Disable();
        playerInput.actions.FindActionMap("UI").Enable();

        // Disable MouseHandler
        mouseHandler.enabled = false;
    }

    public void CloseDialog()
    {
        dialogCanvas.SetActive(false);

        // Switch action maps: Disable UI and enable Player
        playerInput.actions.FindActionMap("Player").Enable();
        playerInput.actions.FindActionMap("UI").Disable();

        // Re-enable MouseHandler
        mouseHandler.enabled = true;
    }

    public void BuyUnit()
    {
        Debug.Log($"Bought {availableUnits[currentUnitIndex]}!");
        // Implement purchase logic here
    }

    public void NextUnit()
    {
        currentUnitIndex = (currentUnitIndex + 1) % availableUnits.Length;
        UpdateUnitDetails();
    }

    public void PreviousUnit()
    {
        currentUnitIndex = (currentUnitIndex - 1 + availableUnits.Length) % availableUnits.Length;
        UpdateUnitDetails();
    }

    private void UpdateUnitDetails()
    {
        statsText.text = $"Unit: {availableUnits[currentUnitIndex]}";
        Debug.Log($"Unit: {availableUnits[currentUnitIndex]}");
    }

    public void OnUICancel(InputAction.CallbackContext context)
    {
        CloseDialog();
    }

    public void OnUISubmit(InputAction.CallbackContext context)
    {
        BuyUnit();
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        float scrollDelta = context.ReadValue<float>();
        if (scrollDelta > 0)
        {
            NextUnit();
        }
        else if (scrollDelta < 0)
        {
            PreviousUnit();
        }
    }

}
