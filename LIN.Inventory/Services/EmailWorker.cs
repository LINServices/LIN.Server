using System.Net;
using System.Net.Mail;

namespace LIN.Inventory.Services;

public class EmailWorker
{


    /// <summary>
    /// Email de salida
    /// </summary>
    private static string Mail { get; set; } = string.Empty;


    /// <summary>
    /// Contraseña del email de salida
    /// </summary>
    private static string Password { get; set; } = string.Empty;


    /// <summary>
    /// Inicia el servicio
    /// </summary>
    public static void StarService()
    {
        Mail = Configuration.GetConfiguration("mail:email");
        Password = Configuration.GetConfiguration("mail:password");
    }



    /// <summary>
    /// Enviar un correo
    /// </summary>
    /// <param name="to">Destinatario</param>
    public static bool SendVerification(string to, string url, string mail)
    {

        // Obtiene la plantilla
        var body = File.ReadAllText("wwwroot/Plantillas/Plantilla.html");

        // Remplaza
        body = body.Replace("@@Titulo", "Verificación de correo electrónico");
        body = body.Replace("@@Subtitulo", $"{mail}");
        body = body.Replace("@@Url", url);
        body = body.Replace("@@Mensaje", "Hemos recibido tu solicitud para agregar una dirección de correo electrónico adicional a tu cuenta. Para completar este proceso, da click en el siguiente botón");
        body = body.Replace("@@ButtonMessage", "Verificar");


        // Envía el email
        return SendMail(to, "Verifica el email", body);

    }



    /// <summary>
    /// Enviar un correo
    /// </summary>
    /// <param name="to">Destinatario</param>
    public static bool SendPassword(string to, string nombre, string url)
    {

        // Obtiene la plantilla
        var body = File.ReadAllText("wwwroot/Plantillas/Plantilla.html");

        // Remplaza
        body = body.Replace("@@Titulo", "Reestablecer contraseña");
        body = body.Replace("@@Subtitulo", $"Hola, {nombre}");
        body = body.Replace("@@Url", url);
        body = body.Replace("@@Mensaje", "Recibimos tu solicitud para restablecer la contraseña de tu cuenta LIN. Para completar este proceso, simplemente haz clic en el siguiente botón");
        body = body.Replace("@@ButtonMessage", "Cambiar contraseña");

        // Envía el email
        return SendMail(to, "Cambiar contraseña", body);

    }



    /// <summary>
    /// Enviar un correo
    /// </summary>
    /// <param name="to">Destinatario</param>
    /// <param name="asunto">Asunto</param>
    /// <param name="body">Cuerpo del correo</param>
    public static bool SendMail(string to, string asunto, string body)
    {
        try
        {
            // Configurar los detalles del correo
            string destinatario = to ?? string.Empty;
            string cuerpo = body ?? string.Empty;

            // Configurar el cliente SMTP de Hotmail/Outlook.com
            SmtpClient smtpClient = new("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Mail, Password)
            };

            // Crear el mensaje
            MailMessage correo = new(Mail, destinatario, asunto, cuerpo)
            {
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            // Enviar el correo
            smtpClient.Send(correo);
            return true;
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }
        return false;
    }



}
