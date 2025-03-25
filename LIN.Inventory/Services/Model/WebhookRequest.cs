namespace LIN.Inventory.Services.Model;

public class WebhookRequest
{
    public int OrderId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusString { get; set; } = string.Empty;
    public PayerRequest? Payer { get; set; }
}

public class PayerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
}