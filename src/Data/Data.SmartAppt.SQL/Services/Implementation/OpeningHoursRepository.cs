using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class OpeningHoursRepository : IOpeningHoursRepository
    {
        protected readonly IDbConnection Connection;

        public OpeningHoursRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(OpeningHoursEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@DayOfWeek", SqlDbType.TinyInt) { Value = entity.DayOfWeek });
            cmd.Parameters.Add(new SqlParameter("@OpenTime", SqlDbType.Time) { Value = entity.OpenTime });
            cmd.Parameters.Add(new SqlParameter("@CloseTime", SqlDbType.Time) { Value = entity.CloseTime });

            var output = new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task DeleteAsync(int hoursId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Value = hoursId });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<IEnumerable<OpeningHoursEntity>> GetAllAsync(int skip = 0, int take = 10)
        {
            await EnsureOpenAsync();

            var list = new List<OpeningHoursEntity>();
            using var cmd = new SqlCommand("core.OpeningHours_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    DayOfWeek = reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                    OpenTime = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                    CloseTime = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
                });
            }

            return list;
        }

        public virtual async Task<IEnumerable<OpeningHoursEntity>> GetByBusinessIdAsync(int businessId)
        {
            await EnsureOpenAsync();

            var result = new List<OpeningHoursEntity>();
            using var cmd = new SqlCommand("core.OpeningHours_GetByBusinessId", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    DayOfWeek = reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                    OpenTime = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                    CloseTime = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
                });
            }

            return result;
        }

        public virtual async Task<OpeningHoursEntity?> GetByBusinessIdAndDowAsync(int businessId, byte dayOfWeek)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_GetByBusinessIdAndDow", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@DayOfWeek", SqlDbType.TinyInt) { Value = dayOfWeek });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    DayOfWeek = reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                    OpenTime = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                    CloseTime = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
                };
            }

            return null;
        }

        public virtual async Task<OpeningHoursEntity?> GetByIdAsync(int hoursId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Value = hoursId });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new OpeningHoursEntity
                {
                    OpeningHoursId = reader.GetInt32(reader.GetOrdinal("OpeningHoursId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    DayOfWeek = reader.GetByte(reader.GetOrdinal("DayOfWeek")),
                    OpenTime = reader.GetTimeSpan(reader.GetOrdinal("OpenTime")),
                    CloseTime = reader.GetTimeSpan(reader.GetOrdinal("CloseTime"))
                };
            }

            return null;
        }

        public virtual async Task UpdateAsync(OpeningHoursEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.OpeningHours_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@OpeningHoursId", SqlDbType.Int) { Value = entity.OpeningHoursId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@DayOfWeek", SqlDbType.TinyInt) { Value = entity.DayOfWeek });
            cmd.Parameters.Add(new SqlParameter("@OpenTime", SqlDbType.Time) { Value = entity.OpenTime });
            cmd.Parameters.Add(new SqlParameter("@CloseTime", SqlDbType.Time) { Value = entity.CloseTime });

            await cmd.ExecuteNonQueryAsync();
        }
    }
}