namespace LIN.Server.Controllers.Api;


[Route("/api/pais")]
public class PaísesController : Controller
{



    /// <summary>
    /// Obtiene la lista de países
    /// </summary>
    [HttpGet]
    public ReadAllResponse<PaisDataModel> GetAll()
    {
        try
        {
            // Obtiene el string
            var text = System.IO.File.ReadAllText("wwwroot/Api/países.json");

            // Serializa
            var lista = JsonConvert.DeserializeObject<List<PaisDataModel>>(text);

            return new(Responses.Success, lista ?? new());
        }
        catch
        {
            return new();
        }

    }



}
