namespace LIN.Server.Controllers;


[Route("user")]
public class UserController : ControllerBase
{


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
            return new(Responses.Unauthorized);
        }


        var rol = (await Data.Users.Read(id)).Model.Rol;


        if (rol != UserRol.Admin)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var response = await Data.Users.GetAll(pattern);

        return response;

    }




}