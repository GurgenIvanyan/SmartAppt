using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class BookingRepository : IBookingRepository
    {
        private readonly IDbConnection _connection;

        public BookingRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await ((SqlConnection)_connection).OpenAsync();
        }

        public virtual async Task CancelAsync(int bookingId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_Cancel", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<int> CreateAsync(BookingEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_SafeCreate", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = entity.ServiceId });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = entity.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@StartAtUtc", SqlDbType.DateTime2) { Value = entity.StartAtUtc });
            cmd.Parameters.Add(new SqlParameter("@EndAtUtc", SqlDbType.DateTime2) { Value = entity.EndAtUtc });
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 500) { Value = !string.IsNullOrEmpty(entity.Notes) ? entity.Notes : DBNull.Value });

            var output = new SqlParameter("@BookingId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task DeleteAsync(int bookingId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_Delete", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<IEnumerable<BookingEntity>> GetAllAsync(int skip = 0, int take = 10)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetAll", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            var bookings = new List<BookingEntity>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bookings.Add(new BookingEntity
                {
                    BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    StartAtUtc = reader.GetDateTime(reader.GetOrdinal("StartAtUtc")),
                    EndAtUtc = reader.GetDateTime(reader.GetOrdinal("EndAtUtc")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                });
            }

            return bookings;
        }

        public virtual async Task<IEnumerable<BookingEntity>> GetAllSpecAsync(BookingFilter filter)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetAllSpec", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = filter.BusinessId ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = filter.ServiceId ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = filter.CustomerId ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 12) { Value = !string.IsNullOrEmpty(filter.Status) ? filter.Status : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Date", SqlDbType.Date) { Value = filter.Date ?? (object)DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = filter.Skip ?? 0 });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = filter.Take ?? 100000 });

            var bookings = new List<BookingEntity>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bookings.Add(new BookingEntity
                {
                    BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    StartAtUtc = reader.GetDateTime(reader.GetOrdinal("StartAtUtc")),
                    EndAtUtc = reader.GetDateTime(reader.GetOrdinal("EndAtUtc")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                });
            }

            return bookings;
        }

        public virtual async Task<BookingEntity?> GetByIdAsync(int bookingId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetById", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = bookingId });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BookingEntity
                {
                    BookingId = bookingId,
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    ServiceId = reader.GetInt32(reader.GetOrdinal("ServiceId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    StartAtUtc = reader.GetDateTime(reader.GetOrdinal("StartAtUtc")),
                    EndAtUtc = reader.GetDateTime(reader.GetOrdinal("EndAtUtc")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                    CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc"))
                };
            }

            return null;
        }

        public virtual async Task UpdateAsync(BookingEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_SafeUpdate", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BookingId", SqlDbType.Int) { Value = entity.BookingId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = entity.ServiceId });
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = entity.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@StartAtUtc", SqlDbType.DateTime2) { Value = entity.StartAtUtc });
            cmd.Parameters.Add(new SqlParameter("@EndAtUtc", SqlDbType.DateTime2) { Value = entity.EndAtUtc });
            cmd.Parameters.Add(new SqlParameter("@Notes", SqlDbType.NVarChar, 500) { Value = !string.IsNullOrEmpty(entity.Notes) ? entity.Notes : DBNull.Value });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<Dictionary<DateOnly, int>> GetBookingsCountByBusinessAsync(int businessId, int serviceId, DateOnly startDate, DateOnly endDate)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Booking_GetBookingsCountByBusinessAndRange", (SqlConnection)_connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });
            cmd.Parameters.Add(new SqlParameter("@ServiceId", SqlDbType.Int) { Value = serviceId });
            cmd.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) });
            cmd.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) });

            var result = new Dictionary<DateOnly, int>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var date = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("BookingDate")));
                var count = reader.GetInt32(reader.GetOrdinal("BookingCount"));
                result[date] = count;
            }

            return result;
        }
    }
}