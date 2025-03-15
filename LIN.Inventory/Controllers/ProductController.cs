namespace LIN.Inventory.Controllers;

[Route("[Controller]")]
[RateLimit(requestLimit: 20, timeWindowSeconds: 60, blockDurationSeconds: 120)]
public class ProductController(IHubService hubService, Persistence.Data.Products productsData, Persistence.Data.Inventories inventoryData, IIam Iam) : ControllerBase
{

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="modelo">Modelo del producto</param>
    [HttpPost]
    [InventoryToken]
    public async Task<HttpCreateResponse> Create([FromBody] ProductModel modelo, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Validar parámetro.
        if (modelo == null)
            return new()
            {
                Response = Responses.InvalidParam,
                Message = "El contenido enviado no tiene el formato JSON correcto."
            };

        // Validar datos del modelo.
        if (modelo.Details == null || modelo.Details.Count <= 0)
            return new()
            {
                Response = Responses.InvalidParam,
                Message = "El producto debe de tener información del detalle."
            };

        // Validar datos del modelo.
        if (modelo.Details.Count > 1)
            return new()
            {
                Response = Responses.InvalidParam,
                Message = "El producto debe de tener solo (1) detalle de información."
            };

        // Comprobaciones
        if (modelo.InventoryId <= 0 || modelo.DetailModel!.Quantity < 0 || modelo.DetailModel.PurchasePrice < 0 || modelo.DetailModel.SalePrice < 0)
            return new(Responses.InvalidParam)
            {
                Message = "Uno o varios parámetros son inválidos."
            };

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = modelo.InventoryId,
            Profile = tokenInfo.ProfileId
        });

        // Estados.
        modelo.Statement = ProductBaseStatements.Normal;
        modelo.DetailModel.Status = ProductStatements.Normal;

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Establecer el modelo.
        modelo.Inventory = new()
        {
            Id = modelo.InventoryId
        };

        // Crear.
        var response = await productsData.Create(modelo);

        // Enviar en tiempo real.
        if (response.Response == Responses.Success)
            await hubService.SendNewProduct(modelo.InventoryId, response.LastId);

        return response;

    }


    /// <summary>
    /// Obtiene todos los productos asociados a un inventario.
    /// </summary>
    /// <param name="id">Id del inventario</param>
    [HttpGet("read/all")]
    [InventoryToken]
    public async Task<HttpReadAllResponse<ProductModel>> ReadAll([FromHeader] int id, [FromHeader] string token)
    {

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);


        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest, InventoryRoles.Supervisor, InventoryRoles.Reader];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Resultado.
        var result = await productsData.ReadAll(id);
        return result;

    }


    /// <summary>
    /// Obtiene un producto por medio de su Id
    /// </summary>
    /// <param name="id">Id del producto</param>
    [HttpGet]
    [InventoryToken]
    public async Task<HttpReadOneResponse<ProductModel>> ReadOne([FromHeader] int id, [FromHeader] string token)
    {

        // Comprobación
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();


        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Product,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest, InventoryRoles.Reader, InventoryRoles.Supervisor];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Resultado.
        var result = await productsData.Read(id);
        return result;

    }


    /// <summary>
    /// Obtiene un producto por medio de un detalle asociado
    /// </summary>
    /// <param name="id">Id del detalle de producto</param>
    [HttpGet("readByDetail")]
    [InventoryToken]
    public async Task<HttpReadOneResponse<ProductModel>> ReadByDetail([FromHeader] int id, [FromHeader] string token)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtener el inventario.
        var inventory = await inventoryData.FindByProductDetail(id);

        // Si hubo un problema.
        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el producto.",
                Response = Responses.Unauthorized
            };


        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Inventory,
            Id = inventory.Model,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest, InventoryRoles.Supervisor, InventoryRoles.Reader];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Resultado.
        var result = await productsData.ReadByDetail(id);
        return result;

    }


    /// <summary>
    /// Actualiza la información de un producto
    /// </summary>
    /// <param name="modelo">Nuevo modelo del producto</param>
    [HttpPut]
    [InventoryToken]
    public async Task<HttpResponseBase> UpdateAll([FromBody] ProductModel modelo, [FromHeader] string token)
    {
        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Product,
            Id = modelo.Id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Encontrar el id del inventario.
        var findInventory = await inventoryData.FindByProduct(modelo.Id);

        // Actualizar.
        ResponseBase response = await productsData.Update(modelo);

        // Si fue correcto.
        if (response.Response == Responses.Success && findInventory.Response == Responses.Success)
        {
            await hubService.SendUpdateProduct(findInventory.Model, modelo.Id);
        }

        return response;
    }


    /// <summary>
    /// Elimina un producto
    /// </summary>
    /// <param name="id">Id del producto</param>
    [HttpDelete]
    [InventoryToken]
    public async Task<HttpResponseBase> Delete([FromHeader] int id, [FromHeader] string token)
    {

        // Parámetros.
        if (id < 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtener el inventario.
        var inventory = await inventoryData.FindByProduct(id);

        // Hubo un error.
        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el producto.",
                Response = Responses.Unauthorized
            };

        // Acceso Iam.
        var iam = await Iam.Validate(new IamRequest()
        {
            IamBy = IamBy.Product,
            Id = id,
            Profile = tokenInfo.ProfileId
        });

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator, InventoryRoles.Supervisor];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Respuesta
        ResponseBase response = await productsData.Delete(id);

        // Si fue correcto.
        if (response.Response == Responses.Success)
            await hubService.SendDeleteProduct(inventory.Model, id);

        return response;

    }

}