using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using MudBlazor.Services;
using Watasu.Components;
using Watasu.Interfaces;
using Watasu.Models;
using Watasu.Service;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

namespace Watasu;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services);
        ConfigureWebHost(builder.WebHost);
        var app = builder.Build();
        ConfigurationApplication(app);
        app.Run();
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorComponents().AddInteractiveServerComponents();
        
        services.AddSingleton<ApplicationConfigurationSettings>(options => 
            {
                // Configure passwords as required
                var applicationConfiguration = new ApplicationConfigurationSettings();
                applicationConfiguration.UploadPassword = "12345";
                applicationConfiguration.DebugKey = "54321";
                return applicationConfiguration;
            });
        services.AddSingleton<IWebService, WebService>();
        services.AddSingleton<IValidationService, ValidationService>();
        services.AddSingleton<IBackgroundWindowService, BackgroundWindowService>();

        services.AddMudServices();
        services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });
    }

    private static void ConfigureWebHost(ConfigureWebHostBuilder webhostBuilder)
    {
        webhostBuilder.UseKestrel(options =>
        {
            options.ListenAnyIP(5000); // HTTP
            options.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
        });
    }
    
    private static void ConfigurationApplication(WebApplication webApplication)
    {
        // Configure the HTTP request pipeline.
        if (!webApplication.Environment.IsDevelopment())
        {
            webApplication.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            webApplication.UseHsts();
        }
        
        // webApplication.UseHttpsRedirection();

        webApplication.UseStaticFiles();
        webApplication.UseAntiforgery();

        webApplication.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        
    }
}