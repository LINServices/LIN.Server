namespace LIN.Inventory.Controllers;

[Route("[Controller]")]
[RateLimit(requestLimit: 10, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class ProfileController(IProfilesRepository profileRepository) : ControllerBase
{

    /// <summary>
    /// Obtiene un usuario por medio del Id
    /// </summary>
    /// <param name="id">Id del usuario</param>
    [HttpGet]
    public async Task<HttpReadOneResponse<ProfileModel>> ReadOneByID([FromQuery] int id)
    {

        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var response = await profileRepository.Read(id);

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
    /// Buscar.
    /// </summary>
    /// <param name="pattern">Patron de b�squeda.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("search")]
    public async Task<HttpReadAllResponse<SessionModel<ProfileModel>>> ReadOneByID([FromQuery] string pattern, [FromHeader] string token)
    {

        // Usuarios.
        var users = await Access.Auth.Controllers.Account.Search(pattern, token);

        // Si hubo un error.
        if (users.Response != Responses.Success)
            return new(users.Response);

        // Mapear los ids de los usuarios.
        var map = users.Models.Select(T => T.Id).ToList();

        // Obtiene el usuario.
        var response = await profileRepository.ReadByAccounts(map);

        // Unir las respuestas.
        var joins = (from Account in users.Models
                     join Profile in response.Models
                     on Account.Id equals Profile.AccountId
                     select new SessionModel<ProfileModel>
                     {
                         Account = Account,
                         Profile = Profile
                     }).ToList();

        // Retorna el resultado.
        return new ReadAllResponse<SessionModel<ProfileModel>>
        {
            Response = Responses.Success,
            Models = joins
        };

    }

}