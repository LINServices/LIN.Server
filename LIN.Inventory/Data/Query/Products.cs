using LIN.Inventory;

namespace LIN.Inventory.Data.Query;


public static class Products
{


    /// <summary>
    /// Obtiene un producto por medio del ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public static IQueryable<ProductDataTransfer> Read(int id, Conexión context)
    {

        // Consulta
        var query = from P in context.DataBase.Productos
                    join PP in context.DataBase.PlantillaProductos on P.Plantilla equals PP.ID
                    where P.ID == id
                    join D in context.DataBase.ProductoDetalles
                    on P.ID equals D.ProductoFK
                    where D.Estado == ProductStatements.Normal
                    select new ProductDataTransfer
                    {
                        ProductID = P.ID,
                        Category = PP.Category,
                        Code = PP.Code,
                        Description = PP.Description,
                        Estado = P.Estado,
                        Image = PP.Image,
                        Name = PP.Name,
                        Inventory = P.Inventory,
                        Plantilla = PP.ID,
                        PrecioCompra = D.PrecioCompra,
                        PrecioVenta = D.PrecioVenta,
                        EstadoDetail = D.Estado,
                        Quantity = D.Quantity,
                        IDDetail = D.ID,
                        Provider = P.Provider
                    };

        return query;

    }



    /// <summary>
    /// Obtiene un producto por medio del detalle
    /// </summary>
    /// <param name="id">ID de el detalle</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public static IQueryable<ProductDataTransfer> ReadByDetail(int id, Conexión context)
    {

        // Consulta
        var query = from P in context.DataBase.Productos
                    join PP in context.DataBase.PlantillaProductos on P.Plantilla equals PP.ID
                    join D in context.DataBase.ProductoDetalles
                    on P.ID equals D.ProductoFK
                    where D.ID == id
                    select new ProductDataTransfer
                    {
                        ProductID = P.ID,
                        Category = PP.Category,
                        Code = PP.Code,
                        Description = PP.Description,
                        Estado = P.Estado,
                        Image = PP.Image,
                        Name = PP.Name,
                        Inventory = P.Inventory,
                        Plantilla = PP.ID,
                        PrecioCompra = D.PrecioCompra,
                        PrecioVenta = D.PrecioVenta,
                        EstadoDetail = D.Estado,
                        Quantity = D.Quantity,
                        IDDetail = D.ID,
                        Provider = P.Provider
                    };

        return query;

    }



    /// <summary>
    /// Obtiene la lista de productos asociados a un inventario
    /// </summary>
    /// <param name="id">ID del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public static IQueryable<ProductDataTransfer> ReadAll(int id, Conexión context)
    {

        var query = from P in context.DataBase.Productos
                    where P.Estado == ProductBaseStatements.Normal
                    join PP in context.DataBase.PlantillaProductos on P.Plantilla equals PP.ID
                    where P.Inventory == id
                    join D in context.DataBase.ProductoDetalles
                    on P.ID equals D.ProductoFK
                    where D.Estado == ProductStatements.Normal
                    select new ProductDataTransfer
                    {
                        ProductID = P.ID,
                        Category = PP.Category,
                        Code = PP.Code,
                        Description = PP.Description,
                        Estado = P.Estado,
                        Image = PP.Image,
                        Name = PP.Name,
                        Inventory = P.Inventory,
                        Plantilla = PP.ID,
                        PrecioCompra = D.PrecioCompra,
                        PrecioVenta = D.PrecioVenta,
                        EstadoDetail = D.Estado,
                        Quantity = D.Quantity,
                        IDDetail = D.ID,
                        Provider = P.Provider
                    };

        return query;
    }


}