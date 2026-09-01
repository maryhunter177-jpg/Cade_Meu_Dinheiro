using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CadeMeuDinheiro.App.Services;

namespace CadeMeuDinheiro.App.ViewModels;

public partial class DashboardViewModel(FinanceApiClient api) : ObservableObject
{
    [ObservableProperty] private decimal balance;
    [ObservableProperty] private decimal income;
    [ObservableProperty] private decimal expenses;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private bool hasError;
    public ObservableCollection<TransactionItemViewModel> Recent { get; } = [];
    public bool HasNoTransactions => !IsBusy && !HasError && Recent.Count == 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return; IsBusy = true; HasError = false;
        try
        {
            var data = await api.GetDashboardAsync(CancellationToken.None);
            Balance = data.Balance; Income = data.Income; Expenses = data.Expenses;
            Recent.Clear(); foreach (var item in data.Recent) Recent.Add(new(item.Description, item.Amount, item.Type, item.OccurredOn));
        }
        catch (OperationCanceledException) { HasError = true; }
        catch (HttpRequestException) { HasError = true; }
        catch (JsonException) { HasError = true; }
        catch (NotSupportedException) { HasError = true; }
        catch (InvalidOperationException) { HasError = true; }
        finally { IsBusy = false; OnPropertyChanged(nameof(HasNoTransactions)); }
    }
    [RelayCommand] private static Task OpenTransactionsAsync() => Shell.Current.GoToAsync("//transactions");
}

public sealed record TransactionItemViewModel(string Description, decimal Amount, int Type, DateOnly OccurredOn)
{
    public string DateText => OccurredOn.ToString("dd MMM", CultureInfo.CurrentCulture);
    public string AmountText => $"{(Type == 1 ? "+" : "-")} R$ {Amount:N2}";
    public Color AmountColor => Type == 1 ? Color.FromArgb("#13A875") : Color.FromArgb("#D64550");
}
