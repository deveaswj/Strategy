using UnityEngine;

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
}
