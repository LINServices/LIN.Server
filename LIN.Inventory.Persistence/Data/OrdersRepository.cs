using LIN.Inventory.Persistence.Extensions;
using LIN.Types.Inventory.Models;
using LIN.Types.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIN.Inventory.Persistence.Data;

public class OrdersRepository(Context.Context context, ILogger<OrdersRepository> logger)
{

    public async Task<CreateResponse> Create(OrderModel model)
    {

        try
        {

            if (model.HoldGroupId > 0)
            {
                model.HoldGroup = new() { Id = model.HoldGroupId };
                model.HoldGroup = context.AttachOrUpdate(model.HoldGroup);
            }

            context.Orders.Add(model);
            await context.SaveChangesAsync();

            return new(Responses.Success, model.Id);
        }
        catch (Exception ex)
        {
        }

        return new();

    }


    public async Task<ResponseBase> Update(int id, string status)
    {

        try
        {

            var xx = await (from o in context.Orders
                      where o.Id == id
                      select o).ExecuteUpdateAsync(t => t.SetProperty(t => t.Status, status));

            return new(Responses.Success);
        }
        catch (Exception ex)
        {
        }

        return new();

    }

    public async Task<ReadAllResponse<OrderModel>> ReadAll(string externalId)
    {

        try
        {
            var x = await (from o in context.Orders
                           where o.ExternalId == externalId
                           select o).ToListAsync();

            return new(Responses.Success, x);
        }
        catch (Exception ex)
        {
        }

        return new();

    }

}