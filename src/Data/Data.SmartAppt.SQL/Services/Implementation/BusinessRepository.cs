using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class BusinessRepository : IBusinessRepository
    {
        protected readonly IDbConnection Connection;

        public BusinessRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(BusinessEntity businessData)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(businessData.Name) ? businessData.Name : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320)
            { Value = !string.IsNullOrEmpty(businessData.Email) ? businessData.Email : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50)
            { Value = !string.IsNullOrEmpty(businessData.Phone) ? businessData.Phone : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@TimeZone", SqlDbType.NVarChar, 100)
            { Value = !string.IsNullOrEmpty(businessData.TimeZoneIana) ? businessData.TimeZoneIana : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@SettingsJson", SqlDbType.NVarChar, -1)
            { Value = !string.IsNullOrEmpty(businessData.SettingsJson) ? businessData.SettingsJson : DBNull.Value });

            var output = new SqlParameter("@BusinessId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task<BusinessEntity?> GetByIdAsync(int businessId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BusinessEntity
                {
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    TimeZoneIana = reader.GetString(reader.GetOrdinal("TimeZone")),
                    SettingsJson = reader.IsDBNull(reader.GetOrdinal("SettingsJson")) ? null : reader.GetString(reader.GetOrdinal("SettingsJson")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                };
            }

            return null;
        }

        public virtual async Task<IEnumerable<BusinessEntity>> GetAllAsync(int skip = 0, int take = 10)
        {
            await EnsureOpenAsync();

            var businesses = new List<BusinessEntity>();
            using var cmd = new SqlCommand("core.Business_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                businesses.Add(new BusinessEntity
                {
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    TimeZoneIana = reader.GetString(reader.GetOrdinal("TimeZone")),
                    SettingsJson = reader.IsDBNull(reader.GetOrdinal("SettingsJson")) ? null : reader.GetString(reader.GetOrdinal("SettingsJson")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                });
            }

            return businesses;
        }

        public virtual async Task UpdateAsync(BusinessEntity businessData)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessData.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(businessData.Name) ? businessData.Name : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320)
            { Value = !string.IsNullOrEmpty(businessData.Email) ? businessData.Email : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50)
            { Value = !string.IsNullOrEmpty(businessData.Phone) ? businessData.Phone : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@TimeZone", SqlDbType.NVarChar, 100)
            { Value = !string.IsNullOrEmpty(businessData.TimeZoneIana) ? businessData.TimeZoneIana : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@SettingsJson", SqlDbType.NVarChar, -1)
            { Value = !string.IsNullOrEmpty(businessData.SettingsJson) ? businessData.SettingsJson : DBNull.Value });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task DeleteAsync(int businessId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Business_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            await cmd.ExecuteNonQueryAsync();
        }
    }
}