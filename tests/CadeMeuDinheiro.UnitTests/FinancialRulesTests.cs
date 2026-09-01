using CadeMeuDinheiro.Domain;
using FluentAssertions;

namespace CadeMeuDinheiro.UnitTests;

public sealed class FinancialRulesTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public void BalanceSubtractsConfirmedExpensesFromConfirmedIncome()
    {
        var transactions = new[]
        {
            New(7200m, TransactionType.Income), New(2350m, TransactionType.Expense),
            New(100m, TransactionType.Expense, TransactionStatus.Pending)
        };
        Money.Balance(transactions).Should().Be(4850m);
    }

    [Theory]
    [InlineData(100, BudgetHealth.Normal)]
    [InlineData(750, BudgetHealth.Attention)]
    [InlineData(900, BudgetHealth.NearLimit)]
    [InlineData(1000, BudgetHealth.Exceeded)]
    [InlineData(1200, BudgetHealth.Exceeded)]
    public void BudgetMapsThresholds(decimal spent, BudgetHealth expected) =>
        new Budget(UserId, 2026, 9, 1000m).Health(spent).Should().Be(expected);

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(1000000000)]
    public void TransactionRejectsInvalidAmounts(decimal amount) =>
        FluentActions.Invoking(() => New(amount, TransactionType.Expense)).Should().Throw<DomainException>();

    [Fact]
    public void TransactionUsesBankersRounding() => New(10.125m, TransactionType.Income).Amount.Should().Be(10.12m);

    private static Transaction New(decimal amount, TransactionType type, TransactionStatus status = TransactionStatus.Confirmed) =>
        new(UserId, CategoryId, "Teste", amount, type, new DateOnly(2026, 9, 1), status);
}
