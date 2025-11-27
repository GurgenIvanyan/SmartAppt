using System.Data;
using Data.SmartAppt.SQL.Services;
using Data.SmartAppt.SQL.Services.Implementation;
using Data.SmartAppt.SQL.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.SmartAppt.SQL.Extensions;

public static class DataLayerRegister
{
    public static IServiceCollection AddDataLayerServices(this IServiceCollection services, IConfiguration config)
    {
      
        services.AddScoped<IDbConnection>(sp =>
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        });

    
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IOpeningHoursRepository, OpeningHoursRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        return services;
    }
}