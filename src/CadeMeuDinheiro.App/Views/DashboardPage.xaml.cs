using CadeMeuDinheiro.App.ViewModels;
namespace CadeMeuDinheiro.App.Views;
public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel vm) { InitializeComponent(); BindingContext = vm; }
    protected override void OnAppearing() { base.OnAppearing(); ((DashboardViewModel)BindingContext).LoadCommand.Execute(null); }
}
