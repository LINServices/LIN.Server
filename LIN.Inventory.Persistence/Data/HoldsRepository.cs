using LIN.Inventory.Persistence.Extensions;
using LIN.Types.Inventory.Enumerations;
using LIN.Types.Inventory.Models;
using LIN.Types.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIN.Inventory.Persistence.Data;

public class HoldsRepository(Context.Context context, ILogger<HoldsRepository> logger)
{


    public async Task<CreateResponse> Create(HoldModel model, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? contextTransaction = null)
    {

        model.DetailModel = new ProductDetailModel() { Id = model.DetailId };
        var transaction = contextTransaction ?? context.Database.BeginTransaction();

        try
        {

            model.DetailModel = context.AttachOrUpdate(model.DetailModel);

            context.Holds.Add(model);
            await context.SaveChangesAsync();

            var product = await (from p in context.ProductoDetalles
                                 where p.Id == model.DetailId
                                 select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Quantity, a => a.Quantity - model.Quantity));

            if (contextTransaction is null)
                transaction.Commit();

            return new(Responses.Success, model.Id);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
        }

        return new();

    }


    public async Task<CreateResponse> Return(int holdId)
    {
        try
        {

            var hold = await (from p in context.Holds
                              where p.Id == holdId
                              && p.Status == Types.Inventory.Enumerations.HoldStatus.None
                              select p).FirstOrDefaultAsync();

            if (hold == null)
            {
                return new(Responses.NotRows);
            }


            var product = await (from p in context.ProductoDetalles
                                 where p.Id == hold.DetailId
                                 select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Quantity, a => a.Quantity + hold.Quantity));

            var holds = await (from p in context.Holds
                               where p.Id == hold.Id
                               select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, HoldStatus.Reversed));

            return new(Responses.Success);
        }
        catch (Exception ex)
        {

        }
        return new();

    }


    public async Task<CreateResponse> Approve(int holdId)
    {
        try
        {

            var hold = await (from p in context.Holds
                              where p.Id == holdId
                              && p.Status == Types.Inventory.Enumerations.HoldStatus.None
                              select p).FirstOrDefaultAsync();

            if (hold == null)
            {
                return new(Responses.NotRows);
            }
            var holds = await (from p in context.Holds
                               where p.Id == hold.Id
                               select p).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, HoldStatus.Approve));

            return new(Responses.Success);
        }
        catch (Exception ex)
        {

        }
        return new();

    }







}