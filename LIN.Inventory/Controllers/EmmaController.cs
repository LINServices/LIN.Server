using LIN.Types.Emma.Models;

namespace LIN.Inventory.Controllers;


[Route("[Controller]")]
public class EmmaController(Data.Profiles profileData, Data.Inventories inventoryData) : ControllerBase
{


    /// <summary>
    /// Consultas para Emma.
    /// </summary>
    /// <param name="tokenAuth">Token de identidad.</param>
    /// <param name="query">Entrada a Emma.</param>
    [HttpPost]
    public async Task<HttpReadOneResponse<ResponseIAModel>> Assistant([FromHeader] string tokenAuth, [FromBody] string query)
    {


        HttpClient client = new();

        client.DefaultRequestHeaders.Add("token", tokenAuth);
        client.DefaultRequestHeaders.Add("useDefaultContext", true.ToString().ToLower());


        var request = new LIN.Types.Models.EmmaRequest
        {
            AppContext = "inventory",
            Asks = query
        };



        StringContent stringContent = new(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        var result = await client.PostAsync("http://api.emma.linapps.co/emma", stringContent);


        var ss = await result.Content.ReadAsStringAsync();


        dynamic? fin = Newtonsoft.Json.JsonConvert.DeserializeObject(ss);


        // Respuesta
        return new ReadOneResponse<ResponseIAModel>()
        {
            Model = new()
            {
                IsSuccess = true,
                Content = fin?.result
            },
            Response = Responses.Success
        };

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




        var inventories = await inventoryData.ReadAll(profile.Model.ID);




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