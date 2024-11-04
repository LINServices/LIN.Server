using LIN.Types.Cloud.OpenAssistant.Api;

namespace LIN.Inventory.Controllers;

[Route("[Controller]")]
public class EmmaController(Data.Profiles profileData, Data.Inventories inventoryData, IConfiguration configuration) : ControllerBase
{

    /// <summary>
    /// Respuesta de Emma al usuario.
    /// </summary>
    /// <param name="tokenAuth">Token de identity.</param>
    /// <param name="consult">Consulta del usuario.</param>
    [HttpPost]
    public async Task<HttpReadOneResponse<LIN.Types.Cloud.OpenAssistant.Models.EmmaSchemaResponse>> Assistant([FromHeader] string tokenAuth, [FromBody] string consult)
    {

        // Cliente HTTP.
        HttpClient client = new();

        // Headers.
        client.DefaultRequestHeaders.Add("token", tokenAuth);
        client.DefaultRequestHeaders.Add("useDefaultContext", true.ToString().ToLower());

        // Modelo de Emma.
        var request = new AssistantRequest()
        {
            App = configuration["app:name"],
            Prompt = consult
        };

        // Generar el string content.
        StringContent stringContent = new(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Solicitud HTTP.
        var result = await client.PostAsync(configuration["services:emma"], stringContent);

        // Esperar respuesta.
        var response = await result.Content.ReadAsStringAsync();

        // Objeto.
        var assistantResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ReadOneResponse<Types.Cloud.OpenAssistant.Models.EmmaSchemaResponse>>(response);

        // Respuesta
        return assistantResponse ?? new(Responses.Undefined);

    }


    /// <summary>
    /// Emma IA.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="consult">Prompt.</param>
    [HttpGet]
    public async Task<HttpReadOneResponse<object>> RequestFromEmma([FromHeader] string tokenAuth, [FromHeader] bool includeMethods)
    {

        // Validar token.
        var response = await LIN.Access.Auth.Controllers.Authentication.Login(tokenAuth);


        if (response.Response != Responses.Success)
        {
            return new ReadOneResponse<object>()
            {
                Model = "Este usuario no autenticado en LIN Inventory."
            };
        }

        // 
        var profile = await profileData.ReadByAccount(response.Model.Id);


        if (profile.Response != Responses.Success)
        {
            return new ReadOneResponse<object>()
            {
                Model = "Este usuario no tiene una cuenta en LIN Inventory."
            };
        }




        var inventories = await inventoryData.ReadAll(profile.Model.Id);




        string final = $$""""

                        Eres un sofisticado asesor de ventas, analítico y otras funciones.

                        Estos son datos e inventarios que que el usuario tiene:

                        """";


        foreach (var i in inventories.Models)
        {
            final += $$"""{ Nombre: {{i.Nombre}}, MiRol: {{i.MyRol}}, Id: {{i.ID}} }""" + "\n";
        }


        final += includeMethods ? """
             Estos son comandos, los cuales debes responder con el formato igual a este:
            
            "#Comando(Propiedades en orden separados por coma si es necesario)"
            
            {
              "name": "#select",
              "description": "Abrir un inventario, cuando el usuario se refiera a abrir un inventario",
              "example":"#select(0)",
              "parameters": {
                "properties": {
                  "content": {
                    "type": "number",
                    "description": "Id del inventario"
                  }
                }
              }
            }
            
            {
              "name": "#say",
              "description": "Utiliza esta función para decirle algo al usuario como saludos o responder a preguntas.",
              "example":"#say('Hola')",
              "parameters": {
                "properties": {
                  "content": {
                    "type": "string",
                    "description": "contenido"
                  }
                }
              }
            }
            
            IMPORTANTE:
            No en todos los casos en necesario usar comandos, solo úsalos cuando se cumpla la descripción.
            
            NUNCA debes inventar comandos nuevos, solo puedes usar los que ya existen.
            """ : "\nPuedes contestar con la información de los inventarios del usuario, pero si te piden que hagas algo que no puedes hacer debes responder que en el contexto de la app actual no puedes ejecutar ninguna función";

        return new ReadOneResponse<object>()
        {
            Model = final,
            Response = Responses.Success
        };

    }

}