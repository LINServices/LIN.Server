namespace LIN.Inventory.Services.Implementations;

public class EmailSenderService(IConfiguration configuration) : IEmailSenderService
{

    /// <summary>
    /// Enviar correo.
    /// </summary>
    /// <param name="to">A.</param>
    /// <param name="subject">Asunto.</param>
    /// <param name="body">Cuerpo HTML.</param>
    public async Task<bool> Send(string to, string subject, string body)
    {
        try
        {
            // Servicio.
            Global.Http.Services.Client client = new(configuration["hangfire:mail"])
            {
                TimeOut = 10
            };

            client.AddParameter("subject", subject);
            client.AddParameter("mail", to);

            var result = await client.Post(body);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

}