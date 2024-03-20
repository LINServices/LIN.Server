﻿using LIN.Inventory.Data;
using Newtonsoft.Json.Linq;

namespace LIN.Inventory.Controllers;


[Route("Inventory/access")]
public class InventoryAccessController : ControllerBase
{


    /// <summary>
    /// Obtiene una lista de accesos asociados a un usuario.
    /// </summary>
    /// <param name="id">Id de la cuenta</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<Notificacion>> ReadAll([FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtiene la lista de Id's de inventarios
        var result = await InventoryAccess.ReadAll(tokenInfo.ProfileId);

        // Retorna el resultado
        return result;

    }



    /// <summary>
    /// Cambia el acceso al inventario por medio de su Id
    /// </summary>
    /// <param name="id">Id del estado de inventario</param>
    /// <param name="estado">Nuevo estado del acceso</param>
    [HttpPut("update/state")]
    public async Task<HttpResponseBase> AccessChange([FromHeader] int id, [FromHeader] InventoryAccessState estado)
    {

        // Comprobaciones
        if (id <= 0 || estado == InventoryAccessState.Undefined)
            return new(Responses.InvalidParam);

        // Obtiene la lista de Id's de inventarios
        var result = await InventoryAccess.UpdateState(id, estado);

        // Retorna el resultado
        return result;

    }



    /// <summary>
    /// Obtiene la lista de integrantes asociados a un inventario
    /// </summary>
    /// <param name="inventario">Id del inventario</param>
    /// <param name="usuario">Id del usuario</param>
    [HttpGet("members")]
    public async Task<HttpReadAllResponse<IntegrantDataModel>> ReadAll([FromHeader] int inventario, [FromHeader] int usuario, [FromHeader] string token)
    {

        // Comprobaciones
        if (inventario <= 0 || usuario <= 0)
            return new(Responses.InvalidParam);

        // Obtiene la lista de Id's de inventarios
        var result = await InventoryAccess.ReadIntegrants(inventario);


        var map = result.Models.Select(T => T.Item2.AccountID).ToList();

        var users = await LIN.Access.Auth.Controllers.Account.Read(map, token);


        var i = (from I in result.Models
                join A in users.Models
                on I.Item2.AccountID equals A.Id
                select new IntegrantDataModel
                {
                    AccessID = I.Item1.ID,
                    InventoryID = I.Item1.Inventario,
                    Nombre = A.Name,
                    Perfil = A.Profile,
                    ProfileID = I.Item2.ID,
                    Rol = I.Item1.Rol,
                    Usuario = A.Identity.Unique
                }).ToList();



        return new(Responses.Success, i);

    }



    /// <summary>
    /// Elimina a alguien de un inventario
    /// </summary>
    /// <param name="inventario">Id del inventario</param>
    /// <param name="usuario">Id del usuario que va a ser eliminado</param>
    /// <param name="me">Id del usuario que esta realizando la operación</param>
    [HttpDelete("delete/one")]
    public async Task<HttpResponseBase> DeleteSomeOne([FromHeader] int inventario, [FromHeader] int usuario, [FromHeader] int me)
    {

        // Comprobaciones
        if (inventario <= 0 || usuario <= 0 || me <= 0)
            return new(Responses.InvalidParam);

        // Obtiene la lista de Id's de inventarios
        var result = await InventoryAccess.DeleteSomeOne(inventario, usuario, me);

        return result;

    }



    /// <summary>
    /// Genera una invitación
    /// </summary>
    [HttpPost("new/invitation")]
    [Obsolete("Test")]
    [InventoryToken]
    public async Task<HttpResponseBase> GenerateInvitaciones([FromHeader] string token, [FromBody] InventoryDataModel modelo)
    {

    
        // Valida los nuevos integrantes y el inventario
        if (modelo.ID <= 0 || modelo.UsersAccess.Count <= 0)
            return new(Responses.InvalidParam);


        // Obtiene la lista de Id's de inventarios
        var result = await InventoryAccess.GenerateInvitation(modelo);

        // Retorna el resultado
        return result;

    }


}
