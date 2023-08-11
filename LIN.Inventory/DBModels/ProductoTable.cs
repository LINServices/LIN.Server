namespace LIN.Inventory.DBModels;


public class ProductoTable
{
    public int ID { get; set; } = 0;

    public ProductBaseStatements Estado { get; set; }

    [Column("PROVEEDOR_FK")]
    public int Provider { get; set; } = 0;

    [Column("INVENTARIO_FK")]
    public int Inventory { get; set; } = 0;

    [Column("PLATILLA_FK")]
    public int Plantilla { get; set; } = 0;

}