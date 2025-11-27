using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class HolidayRepository : IHolidayRepository
    {
        protected readonly IDbConnection Connection;

        public HolidayRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(HolidayEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@HolidayDate", SqlDbType.Date) { Value = entity.HolidayDate.Date });
            cmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(entity.Reason) ? entity.Reason : DBNull.Value });

            var output = new SqlParameter("@HolidayId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task DeleteAsync(int holidayId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@HolidayId", SqlDbType.Int) { Value = holidayId });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<IEnumerable<HolidayEntity>> GetAllAsync(int skip = 0, int take = 10)
        {
            await EnsureOpenAsync();

            var holidays = new List<HolidayEntity>();
            using var cmd = new SqlCommand("core.Holiday_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                holidays.Add(new HolidayEntity
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    HolidayDate = reader.GetDateTime(reader.GetOrdinal("HolidayDate")),
                    Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
                });
            }

            return holidays;
        }

        public virtual async Task<HolidayEntity?> GetByBusinessIdAsync(int businessId, DateTime date)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_GetByBusinessId", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@HolidayDate", SqlDbType.Date) { Value = date.Date });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new HolidayEntity
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BusinessId = businessId,
                    HolidayDate = date,
                    Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
                };
            }

            return null;
        }

        public virtual async Task<HolidayEntity?> GetByIdAsync(int holidayId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@HolidayId", SqlDbType.Int) { Value = holidayId });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new HolidayEntity
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    HolidayDate = reader.GetDateTime(reader.GetOrdinal("HolidayDate")),
                    Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
                };
            }

            return null;
        }

        public virtual async Task UpdateAsync(HolidayEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Holiday_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@HolidayId", SqlDbType.Int) { Value = entity.HolidayId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@HolidayDate", SqlDbType.Date) { Value = entity.HolidayDate.Date });
            cmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(entity.Reason) ? entity.Reason : DBNull.Value });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<List<HolidayEntity>> GetAllByMonthAsync(int businessId, int year, int month)
        {
            await EnsureOpenAsync();

            var holidays = new List<HolidayEntity>();
            using var cmd = new SqlCommand("core.Holiday_GetAllByMonth", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@Year", SqlDbType.Int) { Value = year });
            cmd.Parameters.Add(new SqlParameter("@Month", SqlDbType.Int) { Value = month });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                holidays.Add(new HolidayEntity
                {
                    HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    HolidayDate = reader.GetDateTime(reader.GetOrdinal("HolidayDate")),
                    Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason"))
                });
            }

            return holidays;
        }
    }
}