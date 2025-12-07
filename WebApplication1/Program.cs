using System;
using System.IO;
using Common.Logging;

var builder = WebApplication.CreateBuilder(args);

// 1) Քո custom logging-ի կոնֆիգուրացիան (UI-ի համար էլ)
AppLoggerFactory.Configure(options =>
{
    options
        .AddSink(new DebugLog()) // VS Output window
        .AddSink(new FileLog(
            // Կարաս պապկան փոխես, եթե ուզում ես
            directory: Path.Combine(AppContext.BaseDirectory, "logs")))
        // Որ level-ից վերև գրի
        .SetFilter(e => e.Level >= Common.Logging.LogLevel.Info);
});

// 2) Razor Pages
builder.Services.AddRazorPages();

// 3) HttpClient դեպի API
builder.Services.AddHttpClient("SmartApptApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5213/");
});

var app = builder.Build();

// Փոքր стартовый log
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
