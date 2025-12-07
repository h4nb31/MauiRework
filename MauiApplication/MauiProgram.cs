using MauiApplication.Services.Authentication;
using MauiApplication.Services.Authentication._interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

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
        builder.Services.AddAuthorizationCore();

        //builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<ITokenStoreService, TokenStoreService>();
        builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}