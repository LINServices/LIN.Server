﻿namespace LIN.Inventory.Persistence.Extensions;

public static class EfExtensions
{
    /// <summary>
    /// Adjunta una entidad al contexto si no está siendo rastreada.
    /// Si ya está siendo rastreada, actualiza sus valores con los de la entidad proporcionada.
    /// </summary>
    /// <typeparam name="TEntity">El tipo de la entidad.</typeparam>
    /// <param name="context">El contexto de la base de datos.</param>
    /// <param name="entity">La entidad a adjuntar o actualizar.</param>
    public static TEntity AttachOrUpdate<TEntity>(this DbContext context, TEntity entity) where TEntity : class
    {

        // Obtiene la clave primaria de la entidad
        var key = context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();
        if (key == null)
        {
            throw new InvalidOperationException($"La entidad {typeof(TEntity).Name} no tiene una clave primaria definida.");
        }

        // Construye un objeto anónimo con los valores de la clave primaria
        var keyValues = key.Properties.Select(p =>
            typeof(TEntity).GetProperty(p.Name).GetValue(entity)).ToArray();

        // Busca si la entidad ya está siendo rastreada en el contexto
        var localEntity = context.Set<TEntity>().Local.FirstOrDefault(e =>
        {
            bool isMatch = true;
            for (int i = 0; i < key.Properties.Count; i++)
            {
                var property = key.Properties[i];
                var value1 = typeof(TEntity).GetProperty(property.Name).GetValue(e);
                var value2 = keyValues[i];
                if (!object.Equals(value1, value2))
                {
                    isMatch = false;
                    break;
                }
            }
            return isMatch;
        });

        if (localEntity != null)
        {
            return localEntity;
        }

        // Si no está siendo rastreada, la adjunta al contexto
        context.Attach(entity);
        return entity;
    }
}
