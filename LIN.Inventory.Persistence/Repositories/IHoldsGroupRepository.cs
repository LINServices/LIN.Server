﻿namespace LIN.Inventory.Persistence.Repositories;

public interface IHoldsGroupRepository
{
    Task<CreateResponse> Approve(int holdGroupId);
    Task<CreateResponse> Create(HoldGroupModel model);
    Task<ReadOneResponse<int>> GetInventory(int holdGroupId);
    Task<ReadAllResponse<HoldModel>> GetItems(int holdGroupId);
    Task<ReadOneResponse<HoldGroupModel>> Read(int holdGroupId);
    Task<CreateResponse> Return(int holdGroupId);
    Task<ReadAllResponse<HoldModel>> GetItemsHolds(int inventory);
}