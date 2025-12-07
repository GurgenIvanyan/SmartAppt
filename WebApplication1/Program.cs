using System;
using System.IO;
using Common.Logging;

var builder = WebApplication.CreateBuilder(args);


AppLoggerFactory.Configure(options =>
{
    options
        .AddSink(new DebugLog()) 
        .AddSink(new FileLog(
     
            directory: Path.Combine(AppContext.BaseDirectory, "logs")))
      
        .SetFilter(e => e.Level >= Common.Logging.LogLevel.Info);
});


builder.Services.AddRazorPages();

builder.Services.AddHttpClient("SmartApptApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5213/");
});

var app = builder.Build();


var startupLog = AppLoggerFactory.CreateLogger("Startup");
startupLog.Info("Web UI started");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
