namespace LIN.Inventory.Controllers;


[Route("Bridget")]
public class BridgetController : ControllerBase
{


    [HttpGet("search")]
    public async Task<HttpReadAllResponse<SessionModel<ProfileModel>>> ReadOneByID([FromQuery] string userName, [FromHeader] string token)
    {

        // Usuarios
        var users = await LIN.Access.Auth.Controllers.Account.Search(userName, token, false);

        // Si hubo un error
        if (users.Response != Responses.Success)
            return new(users.Response);


        var map = users.Models.Select(T => T.ID).ToList();


        // Obtiene el usuario
        var response = await Data.Profiles.ReadByAccounts(map);

        var joins = (from Account in users.Models
                    join Profile in response.Models
                    on Account.ID equals Profile.AccountID
                    select new SessionModel<ProfileModel>
                    {
                        Account = Account,
                        Profile = Profile
                    }).ToList();

        // Retorna el resultado
        return new ReadAllResponse<SessionModel<ProfileModel>>
        {
            Response = Responses.Success,
            Models = joins
        };

    }


}