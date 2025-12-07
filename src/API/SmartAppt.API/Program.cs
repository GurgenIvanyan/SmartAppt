using System;
using System.IO;
using Business.SmartAppt.Extensions;
using Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmartAppt.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
          
            AppLoggerFactory.Configure(options =>
            {
                options
                    
                    .AddSink(new DebugLog())
                   
                    .AddSink(new FileLog(
                        directory: Path.Combine(AppContext.BaseDirectory, "logs")))
                    
                    .SetFilter(e => e.Level >= Common.Logging.LogLevel.Info);
            });

     
            var builder = WebApplication.CreateBuilder(args);

      
            builder.Services.AddControllers();

        
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddBusinessLayerServices(builder.Configuration);

            var app = builder.Build();


            var startupLog = AppLoggerFactory.CreateLogger<Program>();
            startupLog.Info("=== SmartAppt.API started ===");

       
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
