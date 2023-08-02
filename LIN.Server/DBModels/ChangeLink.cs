namespace LIN.Server.DBModels;


public class ChangeLink
{
    public int ID { get; set; }

    public string Key { get; set; } = string.Empty;

    public int User { get; set; }

    public ChangeLinkStatus Status { get; set; }

    public DateTime Vencimiento { get; set; }
}


public enum ChangeLinkStatus
{
    None,
    Actived,
    Desactived
}