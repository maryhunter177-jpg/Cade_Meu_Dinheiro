using Microsoft.Extensions.DependencyInjection;

namespace CadeMeuDinheiro.App;
public partial class App : Application
{
    private readonly AppShell shell;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        shell = services.GetRequiredService<AppShell>();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(shell);
}
