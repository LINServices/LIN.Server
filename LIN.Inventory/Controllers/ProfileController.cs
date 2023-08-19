namespace LIN.Inventory.Controllers;


[Route("profile")]
public class ProfileController : ControllerBase
{


    /// <summary>
    /// Obtiene un usuario por medio del ID
    /// </summary>
    /// <param name="id">ID del usuario</param>
    [HttpGet("read/id")]
    public async Task<HttpReadOneResponse<ProfileModel>> ReadOneByID([FromQuery] int id)
    {

        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var response = await Data.Profiles.Read(id);

        // Si es err�neo
        if (response.Response != Responses.Success)
            return new ReadOneResponse<ProfileModel>()
            {
                Response = response.Response,
                Model = new()
            };

        // Retorna el resultado
        return response;

    }



    /// <summary>
    /// Inicia una sesi�n de usuario
    /// </summary>
    /// <param name="user">Usuario �nico</param>
    /// <param name="password">Contrase�a del usuario</param>
    [HttpGet("login")]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> Login([FromQuery] string user, [FromQuery] string password)
    {

        // Comprobaci�n
        if (!user.Any() || !password.Any())
            return new(Responses.InvalidParam);

        // Respuesta de autenticaci�n
        var authResponse = await LIN.Access.Auth.Controllers.Authentication.Login(user, password);

        // Autenticaci�n err�nea
        if (authResponse.Response != Responses.Success)
        {
            return new ReadOneResponse<AuthModel<ProfileModel>>
            {
                Message = "Autenticaci�n fallida",
                Response = authResponse.Response
            };
        }

        // Obtiene el perfil
        var profile = await Data.Profiles.ReadByAccount(authResponse.Model.ID);

        switch (profile.Response)
        {
            case Responses.Success:
                break;
            case Responses.NotExistProfile:
            {
                    var res = await Data.Profiles.Create(new()
                    {
                        Account = authResponse.Model,
                        Profile = new()
                        {
                            AccountID = authResponse.Model.ID,
                            Creaci�n = DateTime.Now
                        }
                    });

                    if (res.Response != Responses.Success)
                    {
                        return new ReadOneResponse<AuthModel<ProfileModel>>
                        {
                            Response = Responses.UnavailableService,
                            Message = "Un error grave ocurri�"
                        };
                    }

                    profile = res;
                    break;
            }
            default:
                return new ReadOneResponse<AuthModel<ProfileModel>>
                {
                    Response = Responses.UnavailableService,
                    Message = "Un error grave ocurri�"
                };
        }


        // Genera el token
        var token = Jwt.Generate(profile.Model);

        return new ReadOneResponse<AuthModel<ProfileModel>>
        {
            Response = Responses.Success,
            Message = "Success",
            Model = new()
            {
                Account = authResponse.Model,
                LINAuthToken = authResponse.Token,
                Profile = profile.Model
            },
            Token = token
        };

    }



    /// <summary>
    /// Inicia una sesi�n de usuario por medio del token
    /// </summary>
    /// <param name="token">Token de acceso</param>
    [HttpGet("LoginWithToken")]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> LoginWithToken([FromHeader] string token)
    {

        // Respuesta de autenticaci�n
        var authResponse = await LIN.Access.Auth.Controllers.Authentication.Login(token);

        // Autenticaci�n err�nea
        if (authResponse.Response != Responses.Success)
        {
            return new ReadOneResponse<AuthModel<ProfileModel>>
            {
                Message = "Autenticaci�n fallida",
                Response = authResponse.Response
            };
        }

        // Obtiene el perfil
        var profile = await Data.Profiles.ReadByAccount(authResponse.Model.ID);



        switch (profile.Response)
        {
            case Responses.Success:
                break;
            case Responses.NotExistProfile:
                {
                    var res = await Data.Profiles.Create(new()
                    {
                        Account = authResponse.Model,
                        Profile = new()
                        {
                            AccountID = authResponse.Model.ID,
                            Creaci�n = DateTime.Now
                        }
                    });

                    if (res.Response != Responses.Success)
                    {
                        return new ReadOneResponse<AuthModel<ProfileModel>>
                        {
                            Response = Responses.UnavailableService,
                            Message = "Un error grave ocurri�"
                        };
                    }

                    profile = res;
                    break;
                }
            default:
                return new ReadOneResponse<AuthModel<ProfileModel>>
                {
                    Response = Responses.UnavailableService,
                    Message = "Un error grave ocurri�"
                };
        }

        // Genera el token
        var tokenGen = Jwt.Generate(profile.Model);

        return new ReadOneResponse<AuthModel<ProfileModel>>
        {
            Response = Responses.Success,
            Message = "Success",
            Model = new()
            {
                Account = authResponse.Model,
                LINAuthToken = authResponse.Token,
                Profile = profile.Model
            },
            Token = tokenGen
        };

    }


}