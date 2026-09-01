using CadeMeuDinheiro.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CadeMeuDinheiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connection = config.GetConnectionString("Database") ?? throw new InvalidOperationException("ConnectionStrings:Database is required.");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connection));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton(TimeProvider.System);
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));
        return services;
    }
}
