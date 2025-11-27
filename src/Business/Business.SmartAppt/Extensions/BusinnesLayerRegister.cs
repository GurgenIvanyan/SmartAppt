using Business.SmartAppt.Services;
using Business.SmartAppt.Services.Implementation;
using Data.SmartAppt.SQL.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Business.SmartAppt.Extensions;

public static class BusinessLayerRegister
{
    public static IServiceCollection AddBusinessLayerServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddDataLayerServices(config);

        return services;
    }
}