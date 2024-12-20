using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum PlayerID { None, Player1, Player2 };

public enum GameInputMode { None, Idle, UnitActive, BuyNewUnit, PlaceNewUnit, GameOver };
// None -- should never be used
// Idle -- select unit, buy new unit, or end turn
//      Buy Unit: Yes
//      Select:   Yes
//      End Turn: Yes
// UnitActive -- unit selected -- use it, select something else, deselect it, or end turn
//      Buy Unit: No
//      Select:   Yes
//      End Turn: Yes
// PlaceNewUnit -- place new unit -- cannot do anything else until done
//      Buy Unit: No
//      Select:   No
//      End Turn: No

public class GameMaster : MonoBehaviour
{
    [SerializeField] GameObject selectionUISquare;
    private Unit selectedUnit;

    [SerializeField] TextMeshProUGUI turnText;
    [SerializeField] GameObject centerTile;
    [SerializeField] GameObject victoryPanel;
    [SerializeField] TextMeshProUGUI playerWonText;

    public PlayerID CurrentPlayer { get { return currentPlayer; } }
    private PlayerID currentPlayer = PlayerID.Player1;
    private int playerTurn = 1;

    PlayerID winningPlayer = PlayerID.None;

    private Tile[] tiles;

    Dictionary<PlayerID, Player> players = new();

    [SerializeField] GameInputMode gameInputMode = GameInputMode.Idle;

    public GameInputMode GameInputMode { get { return gameInputMode; } }

    SelectionIndicator selectionIndicator;

    private StoreDialog storeDialog;

// public enum GameInputMode { None, Idle, UnitActive, BuyNewUnit, PlaceNewUnit, GameOver };
    public bool InIdleMode() => (gameInputMode == GameInputMode.Idle);
    public bool InUnitActiveMode() => (gameInputMode == GameInputMode.UnitActive);
    public bool InBuyMode() => (gameInputMode == GameInputMode.BuyNewUnit);
    public bool InPlaceMode() => (gameInputMode == GameInputMode.PlaceNewUnit);
    public bool InGameOverMode() => (gameInputMode == GameInputMode.GameOver);

    public void SetIdleMode() => gameInputMode = GameInputMode.Idle;
    public void SetUnitActiveMode() => gameInputMode = GameInputMode.UnitActive;
    public void SetBuyMode() => gameInputMode = GameInputMode.BuyNewUnit;
    public void SetPlaceMode() => gameInputMode = GameInputMode.PlaceNewUnit;
    public void SetGameOverMode() => gameInputMode = GameInputMode.GameOver;

    void Awake()
    {
        selectionIndicator = FindObjectOfType<SelectionIndicator>();
        storeDialog = FindObjectOfType<StoreDialog>();
        LoadPlayers();
        SetCurrentPlayer(1);
    }

    void Start()
    {
        tiles = FindObjectsOfType<Tile>();
        GrantIncome();
    }

    public Unit[] GetUnits() => players[currentPlayer].GetUnits();
    public Unit[] GetUnits(PlayerID playerID) => players[playerID].GetUnits();

    // get units of all players that are not the current player
    public Unit[] GetEnemyUnits() => GetEnemyUnits(currentPlayer);
    public Unit[] GetEnemyUnits(PlayerID playerID)
    {
        PlayerID otherPlayer = (playerID == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;
        return players[otherPlayer].GetUnits();
    }

    public Tile[] GetTiles() => tiles;

    void Update()
    {
        turnText.text = "Player " + playerTurn + "'s turn";
        UpdateSelectionUI();
    }

    public void GameOver()
    {
        gameInputMode = GameInputMode.GameOver;
        winningPlayer = currentPlayer;
        Debug.Log("Game Over! Winning player: " + winningPlayer);
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        // Pause for 1 second
        // then show the Game Over canvas
        yield return new WaitForSeconds(1.0f);
        Debug.Log("(Here's where we would show the Game Over canvas)");
        playerWonText.text = "Player " + playerTurn.ToString() + " wins!";
        victoryPanel.SetActive(true);
    }

    public bool IsMousable()
    {
        return (InIdleMode() || InUnitActiveMode() || InPlaceMode());
    }

    public void OpenStore(Vector3 position)
    {
        storeDialog.OpenStore(GetPlayer(), position);
    }

    public void OnEndTurn(InputAction.CallbackContext context)
    {
        if (IsMousable() && context.performed)
        {
            EndTurn();
        }
    }

    public void OnEscape()
    {
        if (InUnitActiveMode())
        {
            DeselectUnit();
        }
        else if (InPlaceMode())
        {
            // CancelNewUnitPlacement();
        }
    }

    public void OnSelectNext()
    {
        if (InIdleMode() || InUnitActiveMode() || InPlaceMode())
        {
            // Select Next/Prev behavior:
            // If no unit is selected, select the active player's first unit
            // If a unit is selected, select the active player's next or previous unit
            string debugPrefix = "OnSelectNext (Player " + currentPlayer + "): ";
            if (selectedUnit == null)
            {
                // Select First
                Debug.Log(debugPrefix + "Select First");
            }
            else
                {
                bool shiftModifier = Keyboard.current.shiftKey.isPressed;
                if (shiftModifier)
                {
                    // Select Previous
                    Debug.Log(debugPrefix + "Select Previous");
                }
                else
                {
                    // Select Next
                    Debug.Log(debugPrefix + "Select Next");
                }
            }
        }
        else
        {
            // Do nothing
        }
    }

    void UpdateSelectionUI()
    {
        if (selectedUnit != null)
        {
            selectionUISquare.SetActive(true);
            selectionUISquare.transform.position = selectedUnit.transform.position;
        }
        else
        {
            selectionUISquare.SetActive(false);
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
        PlayerID playerID = PlayerID.None;
        if (playerNumber == 1) playerID = PlayerID.Player1;
        if (playerNumber == 2) playerID = PlayerID.Player2;
        return GetPlayer(playerID);
    }

    public void GrantIncome() => GrantIncome(currentPlayer);
    public void GrantIncome(PlayerID playerID)
    {
        // get the current player's units
        Player player = GetPlayer(playerID);
        GameObject playerUnits = player.Units;
        IncomeGenerator[] incomeGenerators = playerUnits.GetComponentsInChildren<IncomeGenerator>();

        Debug.Log("Granting income to player " + playerID + " who has " + player.GetCurrency() + " currency");

        int currencyThisTurn = 0;
        foreach (IncomeGenerator incomeGenerator in incomeGenerators)
        {
            int income = incomeGenerator.GetIncome();
            currencyThisTurn += income;
            Debug.Log("Granting " + income + " from " + incomeGenerator.name);
            Debug.Log("Total so far: " + currencyThisTurn);
        }
        player.AddCurrency(currencyThisTurn);
    }

    public void SetHealth(int health, PlayerID playerID) => GetPlayer(playerID).SetHealth(health);
    public void SetHealth(int health) => GetPlayer().SetHealth(health);
    public void SetCurrency(int currency, PlayerID playerID) => GetPlayer(playerID).SetCurrency(currency);
    public void SetCurrency(int currency) => GetPlayer().SetCurrency(currency);


    public void EndTurn()
    {
        Debug.Log("End Turn for player " + currentPlayer);
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

    public void SetTileFocus(Transform tileTransform)
    {
        // move the SelectionIndicator to the tile
        // selectionIndicator.MoveTo(tileTransform.position);
    }

    void ResetUnits()
    {
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            unit.ResetUnit(currentPlayer);
        }
    }

    public void SelectUnit(Unit unit)
    {
        Debug.Log("Selecting unit: " + unit.name);
        selectedUnit = unit;
        selectedUnit.Select();
        SetUnitActiveMode();
        Debug.Log("Game Input Mode: " + gameInputMode);
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
            selectedUnit.Deselect();
            selectedUnit = null;
            SetIdleMode();
            Debug.Log("Game Input Mode: " + gameInputMode);
        }
    }

    public void AttackUnit(Unit target)
    {
        if (selectedUnit != null)
        {
            if (target != null && target.IsAttackable())
            {
                selectedUnit.Attack(target);

                if (target == null) return;     // null if target unit died
                target.CounterAttack(selectedUnit);
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}
