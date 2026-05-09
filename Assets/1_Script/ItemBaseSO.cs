using UnityEngine;

public enum ItemType { Consumable, Material, Equipment }

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
}
