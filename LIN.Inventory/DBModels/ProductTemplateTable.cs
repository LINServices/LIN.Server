namespace LIN.Inventory.DBModels;


public class ProductTemplateTable
{
    public int ID { get; set; } = 0;

    [Column("IMAGEN")]
    public byte[] Image { get; set; } = Array.Empty<byte>();

    [Column("NOMBRE")]
    public string Name { get; set; } = string.Empty;

    [Column("CODIGO")]
    public string Code { get; set; } = string.Empty;

    [Column("DESCRIPCION")]
    public string Description { get; set; } = string.Empty;

    [Column("CATEGORIA")]
    public ProductCategories Category { get; set; } = ProductCategories.Undefined;

}
