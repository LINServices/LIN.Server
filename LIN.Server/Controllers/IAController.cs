namespace LIN.Server.Controllers;


[Route("IA")]
public class IAController : ControllerBase
{


    /// <summary>
    /// Predice el genero de un nombre
    /// </summary>
    /// <param name="name">Nombre</param>
    [HttpGet("names")]
    public async Task<ActionResult<dynamic>> PredictName([FromQuery] string name, [FromHeader] string token)
    {

        // Validación del token
        var (isValid, _, _) = Jwt.Validate(token);

        if (!isValid)
            return Unauthorized(new { Message = "Token invalido" });

        // Validación del nombre
        if (name == null || name.Length <= 0)
            return Responses.InvalidParam;

        // IA Nombre (Genero)
        try
        {
            var res = await Developers.IAName(name);
            return res;
        }
        catch
        {
        }

        return "Error desconocido";
    }



    /// <summary>
    /// Predice la categoría de una imagen
    /// </summary>
    /// <param name="imageByte">Arreglo bytes de la imagen</param>
    [HttpPost("image")]
    public async Task<ReadOneResponse<ProductCategories>> PredictImage([FromBody] byte[] imageByte)
    {


        if (imageByte.Length <= 0)
            return new(Responses.Success, ProductCategories.Undefined);

        try
        {
            return await Developers.IAVision(imageByte);
        }
        catch
        {
        }
        return new();
    }



}
