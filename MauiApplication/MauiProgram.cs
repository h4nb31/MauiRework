using MauiApplication.Services.Authentication;
using MauiApplication.Services.Authentication._interfaces;
using MauiApplication.Services.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Monitor.Services._interfaces.Web;

namespace MauiApplication;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddHttpClient();
        builder.Services.AddAuthorizationCore();

        //builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<IJwtService, JwtService>();
        
        builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
        builder.Services.AddScoped(provider => (AuthStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());

        builder.Services.AddScoped<ITokenStoreService, TokenStoreService>();
        builder.Services.AddScoped<IRequestService, RequestService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}