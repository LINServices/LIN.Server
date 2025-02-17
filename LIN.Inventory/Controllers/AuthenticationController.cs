namespace LIN.Inventory.Controllers;

[Route("[Controller]")]
[RateLimit(requestLimit: 10, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class AuthenticationController(Persistence.Data.Profiles profilesData) : ControllerBase
{

    /// <summary>
    /// Iniciar sesión.
    /// </summary>
    /// <param name="user">Usuario único.</param>
    /// <param name="password">Contraseña.</param>
    [HttpGet("credentials")]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> Login([FromQuery] string user, [FromQuery] string password)
    {

        // Validar parámetros.
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            return new()
            {
                Message = "Usuario / contraseña vacíos o tienen un formato incorrecto.",
                Response = Responses.InvalidParam
            };

        // Respuesta de Cloud Identity.
        var authResponse = await Access.Auth.Controllers.Authentication.Login(user, password);

        // Autenticación errónea.
        if (authResponse.Response != Responses.Success)
            return new ReadOneResponse<AuthModel<ProfileModel>>
            {
                Message = "Autenticación fallida",
                Response = authResponse.Response
            };

        // Login en contactos.
        var contactsLogin = Access.Contacts.Controllers.Profiles.Login(authResponse.Token);

        // Obtiene el perfil.
        var profile = await profilesData.ReadByAccount(authResponse.Model.Id);

        // Segun.
        switch (profile.Response)
        {
            // Correcto.
            case Responses.Success:
                break;

            // Si el perfil no existe.
            case Responses.NotExistProfile:
                {

                    // Crear el perfil.
                    var createResponse = await profilesData.Create(new()
                    {
                        Account = authResponse.Model,
                        Profile = new()
                        {
                            AccountId = authResponse.Model.Id,
                            Creation = DateTime.Now
                        }
                    });

                    // Si hubo un error.
                    if (createResponse.Response != Responses.Success)
                        return new ReadOneResponse<AuthModel<ProfileModel>>
                        {
                            Response = Responses.UnavailableService,
                            Message = "Un error grave ocurrió"
                        };

                    // Establecer.
                    profile = createResponse;
                    break;
                }

            // Otros errores.
            default:
                return new ReadOneResponse<AuthModel<ProfileModel>>
                {
                    Response = Responses.UnavailableService,
                    Message = "Un error grave ocurrió"
                };
        }

        // Genera el token
        var token = Jwt.Generate(profile.Model);

        // Respuesta.
        await contactsLogin;

        return new ReadOneResponse<AuthModel<ProfileModel>>
        {
            Response = Responses.Success,
            Message = "Success",
            Model = new()
            {
                Account = authResponse.Model,
                TokenCollection = new()
                {
                    {"identity", authResponse.Token },
                    {"contacts", contactsLogin.Result.Token }
                },
                Profile = profile.Model
            },
            Token = token
        };

    }


    /// <summary>
    /// Iniciar sesión de usuario por medio del token.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("token")]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> LoginWithToken([FromHeader] string token)
    {

        // Login en contactos
        var contactsLogin = Access.Contacts.Controllers.Profiles.Login(token);

        // Respuesta de autenticación
        var authResponse = await Access.Auth.Controllers.Authentication.Login(token);

        // Autenticación errónea
        if (authResponse.Response != Responses.Success)
            return new()
            {
                Message = "Autenticación fallida",
                Response = authResponse.Response
            };

        // Obtiene el perfil
        var profile = await profilesData.ReadByAccount(authResponse.Model.Id);

        // Esperar la respuesta en contactos.
        await contactsLogin;

        // Validar respuesta.
        switch (profile.Response)
        {

            // Correcto.
            case Responses.Success:
                break;

            // Si el perfil no existe.
            case Responses.NotExistProfile:
                {

                    // Crear el perfil.
                    var createResponse = await profilesData.Create(new()
                    {
                        Account = authResponse.Model,
                        Profile = new()
                        {
                            AccountId = authResponse.Model.Id,
                            Creation = DateTime.Now
                        }
                    });

                    // Validar.
                    if (createResponse.Response != Responses.Success)
                        return new ReadOneResponse<AuthModel<ProfileModel>>
                        {
                            Response = Responses.UnavailableService,
                            Message = "Un error grave ocurrió"
                        };

                    // Establecer.
                    profile = createResponse;
                    break;
                }

            // Otros casos.
            default:
                return new()
                {
                    Response = Responses.UnavailableService,
                    Message = "Un error grave ocurrió"
                };
        }

        // Genera el token
        var tokenGen = Jwt.Generate(profile.Model);

        // Respuesta.
        return new ReadOneResponse<AuthModel<ProfileModel>>
        {
            Response = Responses.Success,
            Message = "Success",
            Model = new()
            {
                Account = authResponse.Model,
                TokenCollection = new()
                {
                    {"identity", authResponse.Token },
                    {"contacts", contactsLogin.Result.Token }
                },
                Profile = profile.Model
            },
            Token = tokenGen
        };

    }

}