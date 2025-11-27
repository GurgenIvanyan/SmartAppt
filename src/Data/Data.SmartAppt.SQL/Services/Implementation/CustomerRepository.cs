using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Data.SmartAppt.SQL.Services.Implementation
{
    public class CustomerRepository : ICustomerRepository
    {
        protected readonly IDbConnection Connection;

        public CustomerRepository(IDbConnection connection)
        {
            Connection = connection;
        }

        protected virtual async Task EnsureOpenAsync()
        {
            if (Connection.State != ConnectionState.Open)
                await ((SqlConnection)Connection).OpenAsync();
        }

        public virtual async Task<int> CreateAsync(CustomerEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Customer_Create", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(entity.FullName) ? entity.FullName : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320)
            { Value = !string.IsNullOrEmpty(entity.Email) ? entity.Email : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50)
            { Value = !string.IsNullOrEmpty(entity.Phone) ? entity.Phone : DBNull.Value });

            var output = new SqlParameter("@CustomerId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(output);

            await cmd.ExecuteNonQueryAsync();
            return Convert.ToInt32(output.Value);
        }

        public virtual async Task DeleteAsync(int customerId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Customer_Delete", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = customerId });

            await cmd.ExecuteNonQueryAsync();
        }

        public virtual async Task<IEnumerable<CustomerEntity>> GetAllAsync(int skip = 0, int take = 10)
        {
            await EnsureOpenAsync();

            var customers = new List<CustomerEntity>();
            using var cmd = new SqlCommand("core.Customer_GetAll", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Skip", SqlDbType.Int) { Value = skip });
            cmd.Parameters.Add(new SqlParameter("@Take", SqlDbType.Int) { Value = take });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                customers.Add(new CustomerEntity
                {
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone"))
                });
            }

            return customers;
        }

        public virtual async Task<IEnumerable<CustomerEntity>> GetByBusinessIdAsync(int businessId)
        {
            await EnsureOpenAsync();

            var customers = new List<CustomerEntity>();
            using var cmd = new SqlCommand("core.Customer_GetByBusinessId", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = businessId });

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                customers.Add(new CustomerEntity
                {
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone"))
                });
            }

            return customers;
        }

        public virtual async Task<CustomerEntity?> GetByIdAsync(int customerId)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Customer_GetById", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = customerId });

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CustomerEntity
                {
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    BusinessId = reader.GetInt32(reader.GetOrdinal("BusinessId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone"))
                };
            }

            return null;
        }

        public virtual async Task UpdateAsync(CustomerEntity entity)
        {
            await EnsureOpenAsync();

            using var cmd = new SqlCommand("core.Customer_Update", (SqlConnection)Connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@CustomerId", SqlDbType.Int) { Value = entity.CustomerId });
            cmd.Parameters.Add(new SqlParameter("@BusinessId", SqlDbType.Int) { Value = entity.BusinessId });
            cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200)
            { Value = !string.IsNullOrEmpty(entity.FullName) ? entity.FullName : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320)
            { Value = !string.IsNullOrEmpty(entity.Email) ? entity.Email : DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50)
            { Value = !string.IsNullOrEmpty(entity.Phone) ? entity.Phone : DBNull.Value });

            await cmd.ExecuteNonQueryAsync();
        }
    }
}