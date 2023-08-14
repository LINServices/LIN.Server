using LIN.Inventory.Data;

namespace LIN.Inventory.Hubs;


public class InventoryHub : Hub
{


    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }



    /// <summary>
    /// Elimina un usuario de un grupo
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }



    /// <summary>
    /// Agrega un nuevo producto
    /// </summary>
    public async Task AddProduct(string groupName, int productID)
    {

        // Busca el nuevo modelo
        var modelo = await Products.Read(productID);

        if (modelo.Response != Responses.Success)
            return;

        await Clients.Group(groupName).SendAsync("SendProduct", modelo.Model);

    }





    public async Task UpdateProduct(string groupName, int productID)
    {

        // Busca el nuevo modelo
        var modelo = await new ProductController().ReadOne(productID);

        if (modelo.Response != Responses.Success)
            return;

        string[] ignored = { Context.ConnectionId };

        await Clients.GroupExcept(groupName, ignored).SendAsync("UpdateProduct", modelo.Object);

    }




    /// <summary>
    /// Agrega un nuevo producto
    /// </summary>
    public async Task RemoveProducto(string groupName, int productID)
    {
        await Clients.Group(groupName).SendAsync("DeleteProducto", productID);
    }





    /// <summary>
    /// Agrega una nueva entrada
    /// </summary>
    public async Task AddEntrada(string groupName, int entradaID)
    {

        // Busca el nuevo modelo
        var modelo = await new InflowController().ReadOne(entradaID, true);

        if (modelo.Response != Responses.Success)
            return;

        var res = modelo.Object as ReadOneResponse<LIN.Shared.Models.InflowDataModel>;

        if (res != null)
            await Clients.Group(groupName).SendAsync("SendInflow", res.Model);

    }



    /// <summary>
    /// Agrega una nueva salida
    /// </summary>
    public async Task AddSalida(string groupName, int salidaID)
    {

        // Busca el nuevo modelo
        var modelo = await new OutflowController().ReadOne(salidaID, true);

        if (modelo.Response != Responses.Success)
            return;

        var res = modelo.Object as ReadOneResponse<LIN.Shared.Models.OutflowDataModel>;

        if (res != null)
            await Clients.Group(groupName).SendAsync("SendOutflow", res.Model);

    }




}