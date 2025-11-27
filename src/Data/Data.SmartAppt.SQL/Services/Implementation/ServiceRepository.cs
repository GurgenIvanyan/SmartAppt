using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class ServiceRepository : IServiceRepository
    {
        protected readonly IDbConnection Connection;

        public ServiceRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(ServiceEntity service)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = service.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = string.IsNullOrEmpty(service.Name) ? DBNull.Value : service.Name });
            cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = service.DurationMin });
            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = service.IsActive });
            cmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = service.Price
            });

            var output = new SqlParameter("@ServiceId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task<ServiceEntity?> GetByIdAsync(int serviceId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ServiceEntity
                {
                    ServiceId = serviceId,
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    DurationMin = reader.GetInt32(reader.GetOrdinal("DurationMin")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                };
            }

            return null;
        }

        public virtual async Task<IEnumerable<ServiceEntity>> GetAllAsync(int skip = 0, int take = 10)
        {
            await EnsureOpenAsync();

            var services = new List<ServiceEntity>();
            using var cmd = new SqlCommand("core.Service_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                services.Add(new ServiceEntity
                {
                    ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    DurationMin = reader.GetInt32(reader.GetOrdinal("DurationMin")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                });
            }

            return services;
        }

        public virtual async Task UpdateAsync(ServiceEntity service)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = service.ServiceId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = service.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = string.IsNullOrEmpty(service.Name) ? DBNull.Value : service.Name });
            cmd.Parameters.Add(new SqlParameter("@DurationMin", SqlDbType.Int) { Value = service.DurationMin });
            cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = service.IsActive });
            cmd.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 2,
                Value = service.Price
            });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task DeleteAsync(int serviceId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Service_Delete", (SqlConnection)Connection);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });

            await cmd.ExecuteNonQueryAsync();
        }
    }
}