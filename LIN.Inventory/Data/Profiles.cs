﻿namespace LIN.Inventory.Data;


public static class Profiles
{



    #region Abstracciones


    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="data">Modelo del usuario</param>
    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Create(data, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    public async static Task<ReadOneResponse<ProfileModel>> Read(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Read(id, context);
        context.CloseActions(connectionKey);
        return res;

    }


    #endregion



    /// <summary>
    /// Crea un nuevo perfil de inventario
    /// /// </summary>
    /// <param name="data">Modelo</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data, Conexión context)
    {

        data.Profile.ID = 0;

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                context.DataBase.Profiles.Add(data.Profile);
                context.DataBase.SaveChanges();

                // Creación del modelo Contacto
                ContactDataModel contacto = new()
                {
                    Name = data.Account.Nombre,
                    Picture = data.Account.Perfil,
                    Mail = "Sin definir",
                    Direction = "Sin definir",
                    Phone = "Sin definir",
                    ProfileID = data.Profile.ID
                };

                context.DataBase.Contactos.Add(contacto);


                // Creación del inventario
                InventoryDataModel inventario = new()
                {
                    Creador = data.Profile.ID,
                    Nombre = "Mi Inventario",
                    Direccion = $"Inventario personal de {data.Account.Usuario}",
                    UltimaModificacion = DateTime.Now
                };

                await context.DataBase.Inventarios.AddAsync(inventario);
                context.DataBase.SaveChanges();

                // Acceso a inventario
                InventoryAcessDataModel acceso = new()
                {
                    Fecha = DateTime.Now,
                    Inventario = inventario.ID,
                    State = InventoryAccessState.Accepted,
                    Rol = InventoryRoles.Administrator,
                    ProfileID = data.Profile.ID
                };

                context.DataBase.AccesoInventarios.Add(acceso);
                context.DataBase.SaveChanges();
                transaction.Commit();


                return new(Responses.Success, data.Profile);

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                ServerLogger.LogError(ex.Message);
                if ((ex.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint") ?? false) || (ex.InnerException?.Message.Contains("duplicate key") ?? false))
                    return new(Responses.ExistAccount);
            }
        }
        return new();
    }



    /// <summary>
    /// Obtiene un perfil
    /// </summary>
    /// <param name="id">ID del perfil</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ProfileModel>> Read(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = await Query.Profiles.Read(id, context).FirstOrDefaultAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotExistAccount);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



}