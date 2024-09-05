using LIN.Types.Contacts.Models;

namespace LIN.Inventory.Controllers;


[Route("[Controller]")]
public class ContactsController : ControllerBase
{


    /// <summary>
    /// Crear nuevo contacto.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso a Contactos.</param>
    [HttpPost]
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
    /// Obtiene los contactos.
    /// </summary>
    /// <param name="token">Token de acceso a Contactos.</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<ContactModel>> ReadAll([FromHeader] string token)
    {

        // Obtiene el usuario
        var result = await LIN.Access.Contacts.Controllers.Contacts.ReadAll(token);

        // Retorna el resultado
        return result ?? new();

    }




    /// <summary>
    /// Obtener un contacto.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <param name="token">Token de acceso a Contactos.</param>
    [HttpGet]
    public async Task<HttpReadOneResponse<ContactModel>> ReadOne([FromHeader] int id, [FromHeader] string token)
    {

        // Obtiene el usuario
        var result = await LIN.Access.Contacts.Controllers.Contacts.Read(id, token);

        // Retorna el resultado
        return result ?? new();

    }




    /// <summary>
    /// Actualiza la información de un contacto.
    /// </summary>
    /// <param name="modelo">Modelo del contacto.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPatch]
    public async Task<HttpResponseBase> Update([FromBody] ContactModel modelo, [FromHeader] string token)
    {

        var response = await LIN.Access.Contacts.Controllers.Contacts.Update(modelo, token);

        return response;
    }




    /// <summary>
    /// Elimina un contacto.
    /// </summary>
    /// <param name="id">Id del contacto.</param>
    /// <param name="token">Token de acceso a contactos.</param>
    [HttpDelete]
    public async Task<HttpResponseBase> Delete([FromHeader] int id, [FromHeader] string token)
    {

        var response = await LIN.Access.Contacts.Controllers.Contacts.Delete(id, token);

        return response;

    }



}