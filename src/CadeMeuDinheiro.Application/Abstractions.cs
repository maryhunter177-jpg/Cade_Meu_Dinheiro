using CadeMeuDinheiro.Domain;

namespace CadeMeuDinheiro.Application;

public interface IAppDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<Category> Categories { get; }
    IQueryable<Transaction> Transactions { get; }
    IQueryable<Budget> Budgets { get; }
    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPasswordService { string Hash(string password); bool Verify(string password, string hash); }
public interface ITokenService { AuthTokens Create(User user); }
public interface ICurrentUser { Guid UserId { get; } }
public interface IFinancialInsightsService { Task<IReadOnlyList<string>> GetInsightsAsync(Guid userId, CancellationToken cancellationToken); }

public sealed record AuthTokens(string AccessToken, DateTimeOffset ExpiresAt);
public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record TransactionRequest(Guid CategoryId, string Description, decimal Amount, TransactionType Type, DateOnly OccurredOn);
public sealed record TransactionResponse(Guid Id, Guid CategoryId, string Description, decimal Amount, TransactionType Type, DateOnly OccurredOn);
public sealed record DashboardResponse(decimal Balance, decimal Income, decimal Expenses, decimal Savings, IReadOnlyList<TransactionResponse> Recent);
public sealed record Page<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int Total);

public sealed class AppValidationException(string message, IReadOnlyDictionary<string, string[]>? errors = null) : Exception(message)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors ?? new Dictionary<string, string[]>();
}
public sealed class NotFoundException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);
