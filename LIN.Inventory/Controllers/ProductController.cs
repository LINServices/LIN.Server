namespace LIN.Inventory.Controllers;


[Route("product")]
public class ProductController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="modelo">Modelo del producto</param>
    [HttpPost("create")]
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
        if (modelo.InventoryId <= 0 || modelo.DetailModel!.Quantity < 0 || modelo.DetailModel.PrecioCompra < 0 || modelo.DetailModel.PrecioVenta < 0)
            return new(Responses.InvalidParam)
            {
                Message ="Uno o varios parámetros son inválidos."
            };


        // Acceso Iam.
        var iam = await Iam.OnInventory(modelo.InventoryId, tokenInfo.ProfileId);

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator];

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
            ID = modelo.InventoryId
        };

        // Crear.
        var response = await Data.Products.Create(modelo);

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
        var iam = await Iam.OnInventory(id, tokenInfo.ProfileId);

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Resultado.
        var result = await Data.Products.ReadAll(id);
        return result;

    }



    /// <summary>
    /// Obtiene un producto por medio de su Id
    /// </summary>
    /// <param name="id">Id del producto</param>
    [HttpGet("read")]
    [InventoryToken]
    public async Task<HttpReadOneResponse<ProductModel>> ReadOne([FromHeader] int id, [FromHeader] string token)
    {

        // Comprobación
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtener el inventario.
        var inventory = await Data.Inventories.FindByProduct(id);

        // Si hubo un error.
        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el producto.",
                Response = Responses.Unauthorized
            };


        // Acceso Iam.
        var iam = await Iam.OnInventory(inventory.Model, tokenInfo.ProfileId);

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Resultado.
        var result = await Data.Products.Read(id);
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
        var inventory = await Data.Inventories.FindByProductDetail(id);

        // Si hubo un problema.
        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el producto.",
                Response = Responses.Unauthorized
            };


        // Acceso Iam.
        var iam = await Iam.OnInventory(inventory.Model, tokenInfo.ProfileId);

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Member, InventoryRoles.Administrator, InventoryRoles.Guest];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Resultado.
        var result = await Data.Products.ReadByDetail(id);
        return result;

    }



    



    /// <summary>
    /// Actualiza la información de un producto
    /// </summary>
    /// <param name="modelo">Modelo del producto</param>
    /// <param name="isBase">TRUE si solo se actualiza la base, FALSE si se actualiza el detalle</param>
    [HttpPatch("update")]
    public async Task<HttpResponseBase> Update([FromBody] ProductModel modelo, [FromHeader] bool isBase)
    {

        // Comprobaciones
        if (isBase && !modelo.Name.Any() || modelo.Statement == ProductBaseStatements.Undefined)
            return new(Responses.InvalidParam);

        if (!isBase && (modelo.DetailModel.PrecioVenta < 0 || modelo.DetailModel.PrecioCompra < 0))
            return new(Responses.InvalidParam);

        // Respuesta
        ResponseBase response = new();

        //if (isBase)
        //    response = await Data.Products.UpdateBase(modelo);
        //else
        //    response = await Data.Products.UpdateDetail(modelo.ProductID, new()
        //    {
        //        Id = modelo.IDDetail,
        //        PrecioCompra = modelo.PrecioCompra,
        //        PrecioVenta = modelo.PrecioVenta,
        //        ProductoFK = modelo.ProductID,
        //        Quantity = modelo.Quantity
        //    });

        return response ?? new();

    }



    /// <summary>
    /// Actualiza la información de un producto
    /// </summary>
    /// <param name="modelo">Nuevo modelo del producto</param>
    [HttpPut("update")]
    public async Task<HttpResponseBase> UpdateAll([FromBody] ProductModel modelo)
    {

        //// Comprobaciones
        //if (!modelo.Name.Any() || modelo.PrecioCompra < 0 || modelo.PrecioVenta < 0 || modelo.Estado == ProductBaseStatements.Undefined)
        //    return new(Responses.InvalidParam);


        // Respuesta
        ResponseBase response = await Data.Products.Update(modelo);

        return response;

    }



    /// <summary>
    /// Elimina un producto
    /// </summary>
    /// <param name="id">Id del producto</param>
    [HttpDelete("delete")]
    [InventoryToken]
    public async Task<HttpResponseBase> Delete([FromHeader] int id, [FromHeader]string token)
    {

        // Parámetros.
        if (id < 0) 
            return new(Responses.InvalidParam);

        // Información del token.
        var tokenInfo = HttpContext.Items[token] as JwtInformation ?? new();

        // Obtener el inventario.
        var inventory = await Data.Inventories.FindByProduct(id);

        // Hubo un error.
        if (inventory.Response != Responses.Success)
            return new()
            {
                Message = "Hubo un error al obtener el producto.",
                Response = Responses.Unauthorized
            };

        // Acceso Iam.
        var iam = await Iam.OnInventory(inventory.Model, tokenInfo.ProfileId);

        // Roles.
        InventoryRoles[] acceptedRoles = [InventoryRoles.Administrator];

        // Si no tiene permisos.
        if (!acceptedRoles.Contains(iam))
            return new()
            {
                Message = "No tienes privilegios en este inventario.",
                Response = Responses.Unauthorized
            };

        // Respuesta
        ResponseBase response = await Data.Products.Delete(id);

        return response;

    }



}