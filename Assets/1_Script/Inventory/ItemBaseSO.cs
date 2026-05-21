using UnityEngine;

public enum ItemType { Consumable, Material, Equipment }
public enum EquipmentSlot { None, Helmet, Armor, Weapon, Boots, Wings, Amulet }

[CreateAssetMenu(fileName = "NewItem", menuName = "GameData/Item")]
public class ItemBaseSO : ScriptableObject
{
    public string itemID;
    public string itemName;
    public ItemType type;
    public Sprite icon;
    [TextArea] public string description;
    public int value; // Ví dụ: 500 (EXP)
    public int sellPrice; // Giá bán lấy Vàng

    [Header("Equipment Settings")]
    public EquipmentSlot equipSlot;
    public PetTier tier;
    
    [Header("Flat Bonuses")]
    public int bonusHP;
    public int bonusAtkPhy;
    public int bonusAtkMag;
    public int bonusDefPhy;
    public int bonusDefMag;
    public int bonusSpeed;

    [Header("Percent Bonuses (0.1 = 10%)")]
    public float percentHP;    // Dành cho Bội
    public float percentAtk;   // Dành cho Cánh (%atk)
    public float percentSpeed; // Dành cho Cánh (%speed)
}
