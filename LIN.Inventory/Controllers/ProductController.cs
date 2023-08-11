using LIN.Inventory.Data;

namespace LIN.Inventory.Controllers;


[Route("product")]
public class ProductController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="modelo">Modelo del producto</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] ProductDataTransfer modelo)
    {


        // Comprobaciones
        if (modelo.Provider <= 0 || modelo.Inventory <= 0 || modelo.Estado == ProductBaseStatements.Undefined || modelo.Quantity < 0 || modelo.PrecioCompra < 0 || modelo.PrecioVenta < 0)
            return new(Responses.InvalidParam);


        // Comprobaciones con plantilla
        if (modelo.Plantilla < 0 && (!modelo.Name.Any() || !modelo.Description.Any()))
        {
            return new(Responses.InvalidParam);
        }



        // Producto base
        var response = await Products.Create(modelo);

        return response;

    }



    /// <summary>
    /// Obtiene todos los productos asociados a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<ProductDataTransfer>> ReadAll([FromHeader] int id)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        var result = await Products.ReadAll(id);
        return result;

    }



    /// <summary>
    /// Obtiene un producto por medio de su ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    [HttpGet("read")]
    public async Task<HttpReadOneResponse<ProductDataTransfer>> ReadOne([FromHeader] int id)
    {

        // Comprobación
        if (id <= 0)
            return new(Responses.InvalidParam);

        var result = await Products.Read(id);
        return result;

    }



    /// <summary>
    /// Obtiene un producto por medio de un detalle asociado
    /// </summary>
    /// <param name="id">ID del detalle de producto</param>
    [HttpGet("readByDetail")]
    public async Task<HttpReadOneResponse<ProductDataTransfer>> ReadByDetail([FromHeader] int id)
    {
        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        var result = await Products.ReadByDetail(id);
        return result;

    }



    /// <summary>
    /// Obtiene la lista de plantillas
    /// </summary>
    [HttpGet("read/all/templates")]
    public async Task<HttpReadAllResponse<ProductDataTransfer>> ReadAllBy([FromQuery] string template)
    {

        // Comprobaciones
        if (string.IsNullOrEmpty(template))
            return new(Responses.InvalidParam);

        var result = await ProductTemplate.ReadAllBy(template);
        return result;

    }



    /// <summary>
    /// Actualiza la información de un producto
    /// </summary>
    /// <param name="modelo">Modelo del producto</param>
    /// <param name="isBase">TRUE si solo se actualiza la base, FALSE si se actualiza el detalle</param>
    [HttpPatch("update")]
    public async Task<HttpResponseBase> Update([FromBody] ProductDataTransfer modelo, [FromHeader] bool isBase)
    {

        // Comprobaciones
        if (isBase && !modelo.Name.Any() || modelo.Estado == ProductBaseStatements.Undefined)
            return new(Responses.InvalidParam);

        if (!isBase && (modelo.PrecioCompra < 0 || modelo.PrecioVenta < 0))
            return new(Responses.InvalidParam);

        // Respuesta
        ResponseBase response;

        if (isBase)
            response = await Products.UpdateBase(modelo);
        else
            response = await Products.UpdateDetail(modelo.ProductID, new()
            {
                ID = modelo.IDDetail,
                PrecioCompra = modelo.PrecioCompra,
                PrecioVenta = modelo.PrecioVenta,
                ProductoFK = modelo.ProductID,
                Quantity = modelo.Quantity
            });

        return response ?? new();

    }



    /// <summary>
    /// Actualiza la información de un producto
    /// </summary>
    /// <param name="modelo">Nuevo modelo del producto</param>
    [HttpPut("update")]
    public async Task<HttpResponseBase> UpdateAll([FromBody] ProductDataTransfer modelo)
    {

        // Comprobaciones
        if (!modelo.Name.Any() || modelo.PrecioCompra < 0 || modelo.PrecioVenta < 0 || modelo.Estado == ProductBaseStatements.Undefined)
            return new(Responses.InvalidParam);


        // Respuesta
        ResponseBase response = await Products.Update(modelo);

        return response;

    }



    /// <summary>
    /// Elimina un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    [HttpDelete("delete")]
    public async Task<HttpResponseBase> Delete([FromHeader] int id)
    {

        if (id < 0) return new(Responses.InvalidParam);

        // Respuesta
        ResponseBase response = await Products.Delete(id);

        return response;

    }



}