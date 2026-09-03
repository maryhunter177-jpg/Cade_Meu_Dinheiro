using CadeMeuDinheiro.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CadeMeuDinheiro.App;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        DashboardContent.ContentTemplate = new DataTemplate(
            () => services.GetRequiredService<DashboardPage>());
    }
}
