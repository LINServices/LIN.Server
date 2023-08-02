namespace LIN.Server.Data;


public static class Users
{



    #region Abstracciones


    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="data">Modelo del usuario</param>
    public async static Task<ReadOneResponse<UserDataModel>> Create(UserDataModel data)
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
    public async static Task<ReadOneResponse<UserDataModel>> Read(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Read(id, context);
        context.CloseActions(connectionKey);
        return res;

    }



    /// <summary>
    /// Obtiene un usuario
    /// </summary>
    /// <param name="user">Usuario de la cuenta</param>
    public async static Task<ReadOneResponse<UserDataModel>> Read(string user, bool login = false)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Read(user, context, login);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene los primeros 10 usuarios que coincidan con el patron
    /// </summary>
    /// <param name="pattern">Patron a buscar</param>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadAllResponse<UserDataModel>> SearchByPattern(string pattern, int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await SearchByPattern(pattern, id, context);  
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Obtiene los primeros 5 usuarios que coincidan con el patron (ADMIN)
    /// </summary>
    /// <param name="pattern">Patron a buscar</param>
    public async static Task<ReadAllResponse<UserDataModel>> GetAll(string pattern)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await GetAll(pattern, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Elimina una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ResponseBase> Delete(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await Delete(id, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Actualiza la información de una cuenta
    /// </summary>
    /// <param name="modelo">Modelo nuevo de la cuenta</param>
    public async static Task<ResponseBase> Update(UserDataModel modelo)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();
        var res = await Update(modelo, context);
        context.CloseActions(connectionKey);
        return res;
    }



    /// <summary>
    /// Actualiza las credenciales (Contraseña de un usuario)
    /// </summary>
    /// <param name="newData">Nuevas credenciales</param>
    public async static Task<ResponseBase> UpdatePassword(UpdatePasswordModel newData)
    {

        var (context, key) = Conexión.GetOneConnection();

        var res = await UpdatePassword(newData, context);
        context.CloseActions(key);
        return res;

    }



    /// <summary>
    /// Actualiza el estado de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="status">Nuevo estado</param>
    public async static Task<ResponseBase> UpdateState(int id, AccountStatus status)
    {

        var (context, key) = Conexión.GetOneConnection();

        var res = await UpdateState(id, status, context);
        context.CloseActions(key);
        return res;

    }



    /// <summary>
    /// Actualiza el genero de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="gender">Nuevo genero</param>
    public async static Task<ResponseBase> UpdateGender(int id, Sexos gender)
    {

        var (context, key) = Conexión.GetOneConnection();

        var res = await UpdateGender(id, gender, context);
        context.CloseActions(key);
        return res;

    }




    #endregion




    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="data">Modelo del usuario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<UserDataModel>> Create(UserDataModel data, Conexión context)
    {

        data.ID = 0;

        // Ejecución
        try
        {

            var res = await context.DataBase.Usuarios.AddAsync(data);
            context.DataBase.SaveChanges();

            return new(Responses.Success, data);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
            if ((ex.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint") ?? false) || (ex.InnerException?.Message.Contains("duplicate key") ?? false))
                return new(Responses.ExistAccount);

        }

        return new();
    }



    /// <summary>
    /// Crea un nuevo usuario y otros datos de la cuenta
    /// /// </summary>
    /// <param name="data">Modelo del usuario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<UserDataModel>> CreateAccount(UserDataModel data, Conexión context)
    {

        data.ID = 0;

        // Ejecución (Transacción)
        using (var transaction = context.DataBase.Database.BeginTransaction())
        {
            try
            {
                context.DataBase.Usuarios.Add(data);
                context.DataBase.SaveChanges();

                // Creación del modelo Contacto
                ContactDataModel contacto = new()
                {
                    Name = data.Nombre,
                    Picture = data.Perfil,
                    Mail = "Sin definir",
                    Direction = "Sin definir",
                    Phone = "Sin definir",
                    UserID = data.ID
                };

                context.DataBase.Contactos.Add(contacto);


                // Creación del inventario
                InventoryDataModel inventario = new()
                {
                    Creador = data.ID,
                    Nombre = "Mi Inventario",
                    Direccion = $"Inventario personal de {data.Usuario}",
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
                    Rol = InventoryRols.Administrator,
                    Usuario = data.ID
                };

                context.DataBase.AccesoInventarios.Add(acceso);
                context.DataBase.SaveChanges();
                transaction.Commit();


                return new(Responses.Success, data);

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
    /// Obtiene un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<UserDataModel>> Read(int id, Conexión context)
    {

        // Ejecución
        try
        {

            var res = await Query.Users.Read(id, context).FirstOrDefaultAsync();

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



    /// <summary>
    /// Obtiene un usuario
    /// </summary>
    /// <param name="user">Usuario de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<UserDataModel>> Read(string user, Conexión context, bool login)
    {

        // Ejecución
        try
        {

            UserDataModel? dataModel;
            if (!login)
            {
                dataModel = await Query.Users.Read(user, context).FirstOrDefaultAsync();
            }
            else
            {
                dataModel = await Query.Users.Read(user, context).Where(T => T.Estado == AccountStatus.Normal).FirstOrDefaultAsync();
            }

            // Si no existe el modelo
            if (dataModel == null)
                return new(Responses.NotExistAccount);

            return new(Responses.Success, dataModel);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene los primeros 10 usuarios que coincidan con el patron
    /// </summary>
    /// <param name="pattern">Patron a buscar</param>
    /// <param name="id">ID de la cuenta (Contexto)</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<UserDataModel>> SearchByPattern(string pattern, int id, Conexión context)
    {

        // Ejecución
        try
        {
            var res = await context.DataBase.Usuarios
                .Where(T => T.Usuario.ToLower().Contains(pattern.ToLower()) && T.ID != id && T.Visibilidad != UserVisibility.Unvisible)
                .Take(10)
                .Select(u => new UserDataModel()
                {
                    ID = u.ID,
                    Nombre = u.Nombre,
                    Usuario = u.Usuario,
                    Perfil = u.Perfil,
                    Sexo = u.Sexo,
                    Insignia = u.Insignia
                }).ToListAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotRows);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene los primeros 5 usuarios que coincidan con el patron (Admin)
    /// </summary>
    /// <param name="pattern">Patron a buscar</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<UserDataModel>> GetAll(string pattern, Conexión context)
    {

        // Ejecución
        try
        {
            var res = await context.DataBase.Usuarios
                .Where(T => T.Usuario.ToLower().Contains(pattern.ToLower()))
                .Take(5)
                .ToListAsync();

            // Si no existe el modelo
            if (res == null)
                return new(Responses.NotRows);

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Elimina una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Delete(int id, Conexión context)
    {

        // Ejecución
        try
        {
            var user = await context.DataBase.Usuarios.FindAsync(id);

            if (user != null)
            {
                user.Estado = AccountStatus.Deleted;
                context.DataBase.SaveChanges();
            }

            return new(Responses.Success);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Actualiza la información de un usuario
    /// ** No actualiza el Usuario datos sensibles
    /// </summary>
    /// <param name="modelo">Modelo nuevo de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Update(UserDataModel modelo, Conexión context)
    {

        // Ejecución
        try
        {
            var user = await context.DataBase.Usuarios.FindAsync(modelo.ID);

            // Si el usuario no se encontró
            if (user == null || user.Estado != AccountStatus.Normal)
            {
                return new(Responses.NotExistAccount);
            }

            // Nuevos datos
            user.Perfil = modelo.Perfil;
            user.Nombre = modelo.Nombre;
            user.Sexo = modelo.Sexo;

            context.DataBase.SaveChanges();
            return new(Responses.Success);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Actualiza la contraseña de un usuario
    /// </summary>
    /// <param name="newData">Nuevas credenciales</param>
    /// <param name="context">Contexto de conexión con la BD</param>
    public async static Task<ResponseBase> UpdatePassword(UpdatePasswordModel newData, Conexión context)
    {

        // Encontrar el usuario
        var usuario = await (from U in context.DataBase.Usuarios
                             where U.ID == newData.Account
                             select U).FirstOrDefaultAsync();

        // Si el usuario no existe
        if (usuario == null)
        {
            return new ResponseBase(Responses.NotExistAccount);
        }

        // Confirmar contraseña
        var newEncrypted = Shared.Security.EncryptClass.Encrypt(Conexión.SecreteWord + newData.NewPassword);

        // Cambiar Contraseña
        usuario.Contraseña = newEncrypted;

        context.DataBase.SaveChanges();
        return new(Responses.Success);

    }



    /// <summary>
    /// Actualiza el estado de un usuario
    /// </summary>
    /// <param name="user">ID del usuario</param>
    /// <param name="status">Nuevo estado</param>
    /// <param name="context">Contexto de conexión con la BD</param>
    public async static Task<ResponseBase> UpdateState(int user, AccountStatus status, Conexión context)
    {

        // Encontrar el usuario
        var usuario = await (from U in context.DataBase.Usuarios
                             where U.ID == user
                             select U).FirstOrDefaultAsync();

        // Si el usuario no existe
        if (usuario == null)
        { 
            return new ResponseBase(Responses.NotExistAccount);
        }

        // Cambiar Contraseña
        usuario.Estado = status;

        context.DataBase.SaveChanges();
        return new(Responses.Success);

    }



    /// <summary>
    /// Actualiza el genero de un usuario
    /// </summary>
    /// <param name="user">ID del usuario</param>
    /// <param name="genero">Nuevo genero</param>
    /// <param name="context">Contexto de conexión con la BD</param>
    public async static Task<ResponseBase> UpdateGender(int user, Sexos genero, Conexión context)
    {

        // Encontrar el usuario
        var usuario = await (from U in context.DataBase.Usuarios
                             where U.ID == user
                             select U).FirstOrDefaultAsync();

        // Si el usuario no existe
        if (usuario == null)
        {
            return new ResponseBase(Responses.NotExistAccount);
        }

        // Cambiar Contraseña
        usuario.Sexo = genero;

        context.DataBase.SaveChanges();
        return new(Responses.Success);

    }



}