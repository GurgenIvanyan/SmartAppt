﻿using Data.SmartAppt.SQL.Models;


namespace Data.SmartAppt.SQL.Services.Interfaces
{
    public interface IBusinessRepository
    {
        Task<int> CreateAsync(BusinessEntity businessData);
        Task<BusinessEntity?> GetByIdAsync(int businessId);
        Task<IEnumerable<BusinessEntity>> GetAllAsync(int skip = 0, int take = 10);
        Task UpdateAsync(BusinessEntity businessData);
        Task DeleteAsync(int businessId);

    }
}