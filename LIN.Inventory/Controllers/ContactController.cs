using LIN.Types.Contacts.Models;
using Newtonsoft.Json.Linq;

namespace LIN.Inventory.Controllers;


[Route("contact")]
public class ContactController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo contacto
    /// </summary>
    /// <param name="modelo">Modelo del contacto</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] ContactModel modelo, [FromHeader] string token)
    {

        // Comprobación de campos
        if (modelo.Nombre.Length <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el resultado
        var response = await LIN.Access.Contacts.Controllers.Contacts.Create(token, modelo);

        // Retorna el resultado
        return response ?? new();

    }




    /// <summary>
    /// Obtiene los contactos asociados a una cuenta
    /// </summary>
    /// <param name="id">Id de la cuenta</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<ContactModel>> ReadAll([FromHeader] string token)
    {

        // Obtiene el usuario
        var result = await LIN.Access.Contacts.Controllers.Contacts.ReadAll(token);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Actualiza la información de un contacto
    /// </summary>
    /// <param name="modelo">Modelo del contacto</param>
    [HttpPatch("update")]
    public async Task<HttpResponseBase> Update([FromBody] ContactModel modelo)
    {

        //// Comprobación de campos
        //if (modelo.Name.Length <= 0)
        //    return new(Responses.InvalidParam);

        //// Obtiene el usuario
        //var result = await Data.Contacts.Update(modelo);

        //// Retorna el resultado
        //return result ?? new();

        return new();
    }



    /// <summary>
    /// Cuenta cuantos contactos tiene una cuenta
    /// </summary>
    /// <param name="token">Token de acceso</param>
    [HttpGet("count")]
    public async Task<HttpReadOneResponse<int>> Count([FromHeader] string token)
    {

        //var (isValid, _, id) = Jwt.Validate(token);

        //if (!isValid)
        //    return new(Responses.Unauthorized);


        //// Obtiene el usuario
        //var result = await Data.Contacts.Count(id);

        //// Retorna el resultado
        //return result ?? new();
        return new();

    }



    /// <summary>
    /// Elimina un contacto
    /// </summary>
    /// <param name="id">Id del contacto</param>
    /// <param name="token">Token de acceso</param>
    [HttpDelete("delete")]
    public async Task<HttpResponseBase> Delete([FromHeader] int id, [FromHeader] string token)
    {

        //var (isValid, _, _) = Jwt.Validate(token);

        //if (!isValid)
        //    return new(Responses.InvalidParam);

        //// Comprobación de campos
        //if (id <= 0)
        //    return new(Responses.InvalidParam);

        //// Obtiene el usuario
        //var result = await Data.Contacts.UpdateStatus(id, ContactStatus.Deleted);

        //// Retorna el resultado
        //return result ?? new();

        return new();

    }



    /// <summary>
    /// Envía a la papelera un contacto
    /// </summary>
    /// <param name="id">Id del contacto</param>
    /// <param name="token">Token de acceso</param>
    [HttpDelete("trash")]
    public async Task<HttpResponseBase> ToTrash([FromHeader] int id, [FromHeader] string token)
    {

        //var (isValid, _, _) = Jwt.Validate(token);

        //if (!isValid)
        //    return new(Responses.InvalidParam);

        //// Comprobación de campos
        //if (id <= 0)
        //    return new(Responses.InvalidParam);

        //// Obtiene el usuario
        //var result = await Data.Contacts.UpdateStatus(id, ContactStatus.OnTrash);

        //// Retorna el resultado
        //return result ?? new();
        return new();
    }



}