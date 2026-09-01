using System.Net.Http.Json;
namespace CadeMeuDinheiro.App.Services;
public sealed class FinanceApiClient(HttpClient http)
{
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct) => await http.GetFromJsonAsync<DashboardDto>("/api/dashboard", ct) ?? new(0, 0, 0, []);
}
public sealed record DashboardDto(decimal Balance, decimal Income, decimal Expenses, IReadOnlyList<TransactionDto> Recent);
public sealed record TransactionDto(string Description, decimal Amount, int Type, DateOnly OccurredOn);
