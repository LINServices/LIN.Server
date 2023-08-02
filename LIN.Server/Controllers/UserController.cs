namespace LIN.Server.Controllers;


[Route("user")]
public class UserController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    /// <param name="modelo">Modelo del usuario</param>
    [HttpPost("create")]
    public async Task<HttpReadOneResponse<UserDataModel>> Create([FromBody] UserDataModel modelo)
    {

        // Comprobaciones
        if (modelo == null || modelo.Contraseña.Length < 4 || modelo.Nombre.Length <= 0 || modelo.Usuario.Length <= 0)
            return new(Responses.InvalidParam);


        // Organización del modelo
        modelo.ID = 0;
        modelo.Contraseña = Shared.Security.EncryptClass.Encrypt(Conexión.SecreteWord + modelo.Contraseña);
        modelo.Creacion = DateTime.Now;
        modelo.Estado = AccountStatus.Normal;
        modelo.Insignia = Insignias.None;
        modelo.Rol = UserRol.User;
        modelo.Perfil = modelo.Perfil.Length == 0
                               ? System.IO.File.ReadAllBytes("wwwroot/profile.png")
                               : modelo.Perfil;

        // IA Nombre (Genero)
        try
        {
            if (modelo.Sexo == Sexos.Undefined)
            {
                // Consulta
                var sex = await Developers.IAName(modelo.Nombre.Trim().Split(" ")[0]);

                // Manejo
                modelo.Sexo = sex.Model;
            }
        }
        catch
        {
        }

        // Conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // Creación del usuario
        var response = await Data.Users.CreateAccount(modelo, context);

        // Evaluación
        if (response.Response != Responses.Success)
            return new(response.Response);

        context.CloseActions(connectionKey);

        // Genera el token
        var token = Jwt.Generate(response.Model);
        response.Token = token;

        // Retorna el resultado
        return response ?? new();

    }



    /// <summary>
    /// Obtiene un usuario por medio del ID
    /// </summary>
    /// <param name="id">ID del usuario</param>
    [HttpGet("read/id")]
    public async Task<HttpReadOneResponse<UserDataModel>> ReadOneByID([FromQuery] int id)
    {

        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var response = await Data.Users.Read(id);

        // Si es erróneo
        if (response.Response != Responses.Success)
            return new ReadOneResponse<UserDataModel>()
            {
                Response = response.Response,
                Model = new()
            };

        // Retorna el resultado
        return response;

    }



    /// <summary>
    /// Inicia una sesión de usuario
    /// </summary>
    /// <param name="user">Usuario único</param>
    /// <param name="password">Contraseña del usuario</param>
    [HttpGet("login")]
    public async Task<HttpReadOneResponse<UserDataModel>> Login([FromHeader] string user, [FromHeader] string password)
    {

        // Comprobación
        if (!user.Any() || !password.Any())
            return new(Responses.InvalidParam);


        // Obtiene el usuario
        var response = await Data.Users.Read(user, true);

        if (response.Response != Responses.Success)
            return new(response.Response);

        if (response.Model.Estado != AccountStatus.Normal)
            return new(Responses.NotExistAccount);

        if (response.Model.Contraseña != Shared.Security.EncryptClass.Encrypt(Conexión.SecreteWord + password))
            return new(Responses.InvalidPassword);

        // Genera el token
        var token = Jwt.Generate(response.Model);

        // Crea registro del login
        _ = Data.Logins.Create(new()
        {
            Date = DateTime.Now,
            UserID = response.Model.ID
        });

        response.Token = token;
        return response;

    }



    /// <summary>
    /// Inicia una sesión de usuario por medio del token
    /// </summary>
    /// <param name="token">Token de acceso</param>
    [HttpGet("LoginWithToken")]
    public async Task<HttpReadOneResponse<UserDataModel>> LoginWithToken([FromHeader] string token)
    {

        // Valida el token
        (var isValid, var user, var _) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.InvalidParam);


        // Obtiene el usuario
        var response = await Data.Users.Read(user, true);

        if (response.Response != Responses.Success)
            return new(response.Response);

        if (response.Model.Estado != AccountStatus.Normal)
            return new(Responses.NotExistAccount);

        // Crea registro del login
        _ = Data.Logins.Create(new()
        {
            Date = DateTime.Now,
            UserID = response.Model.ID
        });

        response.Token = token;
        return response;

    }



    /// <summary>
    /// Obtiene un usuario por medio de el usuario Unico
    /// </summary>
    /// <param name="user">Usuario</param>
    [HttpGet("read/user")]
    public async Task<HttpReadOneResponse<UserDataModel>> ReadOneByUser([FromQuery] string user)
    {

        if (!user.Any())
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var response = await Data.Users.Read(user);

        // Si es erróneo
        if (response.Response != Responses.Success)
            return new ReadOneResponse<UserDataModel>()
            {
                Response = response.Response,
                Model = new()
            };

        // Retorna el resultado
        return response;

    }



    /// <summary>
    /// Obtiene una lista de 10 usuarios cullo usuario cumpla con un patron
    /// </summary>
    /// <param name="pattern">Patron de búsqueda</param>
    /// <param name="id">ID del usuario que esta buscando</param>
    [HttpGet("searchByPattern")]
    public async Task<HttpReadAllResponse<UserDataModel>> ReadAllSearch([FromHeader] string pattern, [FromHeader] int id)
    {

        // Comprobación
        if (id <= 0 || pattern.Trim().Length <= 0)
            return new(Responses.InvalidParam);


        // Obtiene el usuario
        var response = await Data.Users.SearchByPattern(pattern, id);

        return response;
    }



    /// <summary>
    /// Obtiene una lista de 5 usuarios cullo usuario cumpla con un patron (Solo admins)
    /// </summary>
    /// <param name="pattern">Patron de búsqueda</param>
    /// <param name="id">ID del usuario que esta buscando</param>
    [HttpGet("findAllUsers")]
    public async Task<HttpReadAllResponse<UserDataModel>> ReadAllSearch([FromHeader] string pattern, [FromHeader] string token)
    {

        var (isValid, _, id) = Jwt.Validate(token);


        if(!isValid)
        {
            return new(Responses.DontHavePermissions);
        }


        var rol = (await Data.Users.Read(id)).Model.Rol;


        if (rol != UserRol.Admin)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var response = await Data.Users.GetAll(pattern);

        return response;

    }



    /// <summary>
    /// Actualiza los datos de un usuario
    /// </summary>
    /// <param name="modelo">Nuevo modelo</param>
    [HttpPut("update")]
    public async Task<HttpResponseBase> Update([FromBody] UserDataModel modelo)
    {

        if (modelo.ID <= 0 || modelo.Nombre.Any())
            return new(Responses.InvalidParam);

        return await Data.Users.Update(modelo);

    }



    /// <summary>
    /// Actualiza la contraseña
    /// </summary>
    /// <param name="modelo">Nuevo modelo</param>
    [HttpPatch("update/password")]
    public async Task<HttpResponseBase> Update([FromBody] UpdatePasswordModel modelo)
    {

        if (modelo.Account <= 0 || modelo.OldPassword.Length < 4 || modelo.NewPassword.Length < 4)
            return new(Responses.InvalidParam);


        var actualData = await Data.Users.Read(modelo.Account);

        if (actualData.Response != Responses.Success)
            return new(Responses.NotExistAccount);

        var oldEncrypted = actualData.Model.Contraseña;


        if (oldEncrypted != actualData.Model.Contraseña)
        {
            return new ResponseBase(Responses.InvalidPassword);
        }

        return await Data.Users.UpdatePassword(modelo);

    }



    /// <summary>
    /// Elimina una cuenta
    /// </summary>
    /// <param name="id">ID del usuario</param>
    [HttpDelete("delete")]
    public async Task<HttpResponseBase> Delete([FromQuery] int id)
    {

        if (id <= 0)
            return new(Responses.InvalidParam);

        var response = await Data.Users.Delete(id);
        return response;
    }



    /// <summary>
    /// Actualiza los datos de un usuario
    /// </summary>
    /// <param name="modelo">Nuevo modelo</param>
    [HttpPatch("disable/account")]
    public async Task<HttpResponseBase> Disable([FromBody] UserDataModel user)
    {

        if (user.ID <= 0)
        {
            return new(Responses.ExistAccount);
        }

        // Modelo de usuario de la BD
        var userModel = await Data.Users.Read(user.ID);

        if (userModel.Model.Contraseña != Shared.Security.EncryptClass.Encrypt(Conexión.SecreteWord + user.Contraseña))
        {
            return new(Responses.InvalidPassword);
        }


        return await Data.Users.UpdateState(user.ID, AccountStatus.Disable );

    }



    /// <summary>
    /// Actualiza el genero de un usuario
    /// </summary>
    /// <param name="token">Token de acceso</param>
    /// <param name="genero">Nuevo genero</param>
    [HttpPatch("update/gender")]
    public async Task<HttpResponseBase> UpdateGender([FromHeader] string token, [FromHeader] Sexos genero)
    {


        var (isValid, _, id) = Jwt.Validate(token);


        if (!isValid)
        {
            return new(Responses.DontHavePermissions);
        }

        return await Data.Users.UpdateGender(id, genero);

    }



    /// <summary>
    /// Obtiene los mails asociados a una cuenta
    /// </summary>
    /// <param name="token">Token de acceso</param>
    [HttpGet("mails")]
    public async Task<HttpReadAllResponse<EmailDataModel>> GetMails([FromHeader] string token)
    {

        var (isValid, _, id) = Jwt.Validate(token);

        if (!isValid)
        {
            return new(Responses.DontHavePermissions);
        }

        return await Data.Mails.ReadAll(id);

    }



}