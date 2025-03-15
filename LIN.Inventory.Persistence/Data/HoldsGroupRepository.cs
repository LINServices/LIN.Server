using LIN.Types.Inventory.Models;
using LIN.Types.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LIN.Inventory.Persistence.Data;

public class HoldsGroupRepository(Context.Context context, ILogger<HoldsRepository> logger, HoldsRepository holdsRepository)
{


    public async Task<CreateResponse> Create(HoldGroupModel model)
    {

        using (var transaction = context.Database.BeginTransaction())
        {
            try
            {
                var data = model.Holds;
                model.Holds = [];
                context.HoldGroups.Add(model);

                context.SaveChanges();

                foreach (var e in data)
                {
                    e.GroupModel = model;
                    e.GroupId = model.Id;

                    var res = await holdsRepository.Create(e, transaction);
                }

                transaction.Commit();

                return new(Responses.Success, model.Id);


            }
            catch (Exception ex)
            {
                transaction.Rollback();
            }
        }

        return new();

    }


    public async Task<CreateResponse> Return(int holdGroupId)
    {
        try
        {

            var hold = await (from p in context.HoldGroups
                              where p.Id == holdGroupId
                              select p.Holds).FirstOrDefaultAsync();

            if (hold == null)
            {
                return new(Responses.NotRows);
            }

            foreach (var ee in hold)
            {
                await holdsRepository.Return(ee.Id);
            }

            return new(Responses.Success);
        }
        catch (Exception ex)
        {

        }
        return new();

    }


    public async Task<CreateResponse> Approve(int holdGroupId)
    {
        try
        {

            var hold = await (from p in context.HoldGroups
                              where p.Id == holdGroupId
                              select p.Holds).FirstOrDefaultAsync();

            if (hold == null)
            {
                return new(Responses.NotRows);
            }

            foreach (var ee in hold)
            {
                await holdsRepository.Approve(ee.Id);
            }

            return new(Responses.Success);
        }
        catch (Exception ex)
        {

        }
        return new();

    }



    public async Task<ReadAllResponse<HoldModel>> GetItems(int holdGroupId)
    {
        try
        {

            var hold = await (from p in context.Holds
                              where p.GroupId == holdGroupId
                              select new HoldModel
                              {
                                  DetailModel = p.DetailModel,
                                  Status = p.Status,
                                  Quantity = p.Quantity,
                                  Id = p.Id
                              }).ToListAsync();

            if (hold == null)
            {
                return new(Responses.NotRows);
            }

            return new(Responses.Success, hold);
        }
        catch (Exception ex)
        {

        }
        return new();

    }



    public async Task<ReadOneResponse<int>> GetInventory(int holdGroupId)
    {
        try
        {

            var hold = await (from p in context.Holds
                              where p.GroupId == holdGroupId
                              select p.DetailModel.Product.InventoryId).FirstOrDefaultAsync();

            if (hold <= 0)
            {
                return new(Responses.NotRows);
            }

            return new(Responses.Success, hold);
        }
        catch (Exception ex)
        {

        }
        return new();

    }






}