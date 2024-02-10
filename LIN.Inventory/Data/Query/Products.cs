﻿namespace LIN.Inventory.Data.Query;


public static class Products
{


    /// <summary>
    /// Obtiene un producto por medio del Id
    /// </summary>
    /// <param name="id">Id del producto</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public static IQueryable<ProductModel> Read(int id, Conexión context)
    {

        // Consulta
        var query = from P in context.DataBase.Productos
                    where P.Id == id
                    select new ProductModel
                    {
                        Id = id,
                        Statement = P.Statement,
                        Category = P.Category,
                        Code = P.Code,
                        Description = P.Description,
                        Image = P.Image,
                        Name = P.Name,
                        InventoryId = P.InventoryId,
                        Details = P.Details.Where(t => t.Estado == ProductStatements.Normal).Take(1).ToList(),
                    };

        return query;

    }



    /// <summary>
    /// Obtiene un producto por medio del detalle
    /// </summary>
    /// <param name="id">Id de el detalle</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public static IQueryable<ProductModel> ReadByDetail(int id, Conexión context)
    {

        // Consulta
        var query = from P in context.DataBase.ProductoDetalles
                    where P.Id == id
                    select new ProductModel
                    {
                        Id = id,
                        Statement = P.Product.Statement,
                        Category = P.Product.Category,
                        Code = P.Product.Code,
                        Description = P.Product.Description,
                        Image = P.Product.Image,
                        Name = P.Product.Name,
                        InventoryId = P.Product.InventoryId,
                        Details = new() { P }
                    };

        return query;

    }



    /// <summary>
    /// Obtiene la lista de productos asociados a un inventario
    /// </summary>
    /// <param name="id">Id del inventario</param>
    /// <param name="context">Contexto de conexión</param>
    /// <param name="connectionKey">Llave para cerrar la conexión</param>
    public static IQueryable<ProductModel> ReadAll(int id, Conexión context)
    {

        var query = from P in context.DataBase.Productos
                    where P.Statement == ProductBaseStatements.Normal
                    where P.InventoryId == id

                    select new ProductModel
                    {
                        Id = id,
                        Statement = P.Statement,
                        Category = P.Category,
                        Code = P.Code,
                        Description = P.Description,
                        Image = P.Image,
                        Name = P.Name,
                        InventoryId = P.InventoryId,
                        Details = P.Details.Where(t => t.Estado == ProductStatements.Normal).Take(1).ToList()
                    };

        return query;
    }


}