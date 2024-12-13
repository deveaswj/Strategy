using UnityEngine;

public enum UnitTravelType { None, RoadOnly, RoadPrefer, Offroad };
// RoadOnly: can only move on roads
// RoadPrefer: Movement penalty when offroad
// Offroad: No movement penalty

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "Unit/Unit Stats")]
public class UnitStats : ScriptableObject
{
    [Header("Store")]
    public string unitName = "Unit Name";
    [TextArea] public string storeDescription = "Unit Description (for the Store)";
    public bool isPurchasable = true;
    public int price = 10;

    [Header("Stats")]
    public bool isBoss = false;
    public int maxHealth = 5;
    public UnitTravelType travelType = UnitTravelType.None;
    public int travelRange = 1;     // How many tiles the unit moves per turn
    public int attackRange = 1;
    [Tooltip("Damage when attacking from 1 tile away")]
    public int meleeDamage = 1;   // damage to deal when initiating melee attack
    [Tooltip("Damage when attacking from 2+ tiles away")]
    public int rangedDamage = 1;  // damage to deal when initiating ranged attack
    public int counterRange = 1;  // halve counter-damage if outside this range
    public int armorValue = 0;    // damage reduction when defending

    [Header("Visuals")]
    public Sprite unitIcon;
    public Sprite player1;
    public Sprite player2;

    public bool GetsRoadBonus()
    {
        return (travelType == UnitTravelType.RoadOnly) || (travelType == UnitTravelType.RoadPrefer);
    }

    public int GetAttackDamageByDistance(float distance)
    {
        // return melee if distance is 1
        if (distance == 1) return meleeDamage;
        // if distance is greater than counterRange, return half rangedDamage (or 1 at minimum)
        // else return full rangedDamage
        if (distance > counterRange)
        {
            // return half rangedDamage -- rounded, but at least 1
            return Mathf.Max(Mathf.RoundToInt(rangedDamage * 0.5f), 1);
        }
        else
        {
            // return full rangedDamage
            return rangedDamage;
        }
    }
}
