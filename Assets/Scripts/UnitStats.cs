using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "Unit/Unit Stats")]
public class UnitStats : ScriptableObject
{
    [Header("Store")]
    public string unitName = "Unit Name";
    [TextArea] public string storeDescription = "Unit Description (for the Store)";
    public bool isPurchasable = true;
    public int price = 10;
    public GameObject unitPrefab;

    [Header("Stats")]
    public bool isBoss = false;
    public int maxHealth = 5;
    public int travelRange = 1;     // How many tiles the unit moves per turn
    public int roadsBonus = 0;      // Bonus movement while on roads
    public int attackRange = 1;
    [Tooltip("Damage when attacking from 1 tile away")]
    public int meleeDamage = 1;   // damage to deal when initiating melee attack
    [Tooltip("Damage when attacking from 2+ tiles away")]
    public int rangedDamage = 1;  // damage to deal when initiating ranged attack
    public int counterRange = 1;  // halve counter-damage if outside this range
    public int armorValue = 0;    // damage reduction when defending
    public LayerMask blockedLayersMask;

    [Header("Visuals")]
    public Sprite iconGeneric;
    public Sprite iconPlayer1;
    public Sprite iconPlayer2;

    public bool GetsRoadBonus()
    {
        return (roadsBonus > 0);
    }

    public Sprite GetSpriteForPlayer(PlayerID playerID)
    {
        Sprite returnSprite = iconGeneric;
        if (playerID == PlayerID.Player1) returnSprite = iconPlayer1;
        if (playerID == PlayerID.Player2) returnSprite = iconPlayer2;
        Debug.Log("Returning sprite " + returnSprite.name + " for player " + playerID);
        return returnSprite;
    }

    public int GetAttackDamageByDistance(float distance, bool countering = false)
    {
        Debug.Log((countering ? "Countering " : "Attacking ") + distance + " tiles away");

        // return melee if distance is 1
        if (distance == 1) return meleeDamage;

        // else it's a ranged attack

        if (rangedDamage == 0) return 0;

        // countering? check for out of range
        if (countering && distance > counterRange)
        {
            // return half rangedDamage -- rounded, but at least 1
            return Mathf.Max(Mathf.RoundToInt(rangedDamage * 0.5f), 1);
        }
        else
        {
            // intiating attack, or countering within range
            // return full rangedDamage
            return rangedDamage;
        }
    }
}
