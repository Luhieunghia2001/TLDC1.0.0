using Postgrest.Attributes;
using Postgrest.Models;

[Table("item_templates")]
public class ItemTemplateModel : BaseModel
{
    [PrimaryKey("id")]
    public string id { get; set; }

    [Column("name")]
    public string name { get; set; }

    [Column("item_type")]
    public string itemType { get; set; }

    [Column("sell_price")]
    public int sellPrice { get; set; }

    [Column("effect_value")]
    public int effectValue { get; set; }

    [Column("description")]
    public string description { get; set; }

    [Column("equip_slot")]
    public string equipSlot { get; set; }

    [Column("tier")]
    public string tier { get; set; }

    [Column("bonus_hp")]
    public int bonusHP { get; set; }

    [Column("bonus_atk_phy")]
    public int bonusAtkPhy { get; set; }

    [Column("bonus_atk_mag")]
    public int bonusAtkMag { get; set; }

    [Column("bonus_def_phy")]
    public int bonusDefPhy { get; set; }

    [Column("bonus_def_mag")]
    public int bonusDefMag { get; set; }

    [Column("bonus_speed")]
    public int bonusSpeed { get; set; }

    [Column("percent_hp")]
    public float percentHP { get; set; }

    [Column("percent_atk")]
    public float percentAtk { get; set; }

    [Column("percent_speed")]
    public float percentSpeed { get; set; }
}