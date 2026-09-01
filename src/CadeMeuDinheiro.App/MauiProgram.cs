using CadeMeuDinheiro.App.Services;
using CadeMeuDinheiro.App.ViewModels;
using CadeMeuDinheiro.App.Views;
using Microsoft.Extensions.Logging;

namespace CadeMeuDinheiro.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://localhost:7240") });
        builder.Services.AddSingleton<FinanceApiClient>();
        builder.Services.AddTransient<DashboardViewModel>(); builder.Services.AddTransient<DashboardPage>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
