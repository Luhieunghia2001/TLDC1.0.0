using Postgrest.Attributes;
using Postgrest.Models;

[Table("user_inventory")]
public class InventoryModel : BaseModel
{
    [PrimaryKey("id", false)]
    public string id { get; set; }

    [Column("user_id")]
    public string userId { get; set; }

    [Column("item_id")]
    public string itemId { get; set; }

    [Column("quantity")]
    public int quantity { get; set; }

    [Column("enhancement_level")]
    public int enhancement_level { get; set; }
}
