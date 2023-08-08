namespace LIN.Server.DBModels;


public class EmailLink
{
    public int ID { get; set; }
    public string Key { get; set; } = string.Empty;
    public int Email { get; set; }
    public ChangeLinkStatus Status { get; set; }
    public DateTime Vencimiento { get; set; }

}