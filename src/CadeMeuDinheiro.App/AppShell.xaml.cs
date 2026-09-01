using CadeMeuDinheiro.App.Views;

namespace CadeMeuDinheiro.App;

public partial class AppShell : Shell
{
    public AppShell(DashboardPage dashboardPage)
    {
        InitializeComponent();
        DashboardContent.Content = dashboardPage;
    }
}
