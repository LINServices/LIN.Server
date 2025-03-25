namespace LIN.Inventory.Services.Model;

public class IamRequest
{
    public int Id { get; set; }
    public int Profile { get; set; }
    public IamBy IamBy { get; set; }
}

public enum IamBy
{
    Inventory,
    Product,
    ProductDetail,
    Inflow,
    Outflow,
    Access
}