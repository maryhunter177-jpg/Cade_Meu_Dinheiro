namespace CadeMeuDinheiro.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;
}

public sealed class User : Entity
{
    private User() { }
    public User(string name, string email, string passwordHash)
    {
        Name = Required(name, 120);
        Email = Required(email, 254).ToLowerInvariant();
        PasswordHash = passwordHash;
    }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "BRL";
    public bool EmailVerified { get; private set; }
    public bool IsActive { get; private set; } = true;
    private static string Required(string value, int max) => string.IsNullOrWhiteSpace(value) || value.Trim().Length > max
        ? throw new DomainException("Valor obrigatório ou fora do limite permitido.") : value.Trim();
}

public sealed class Category : Entity
{
    private Category() { }
    public Category(Guid userId, string name, TransactionType type, string icon, string color)
    {
        UserId = userId;
        Name = string.IsNullOrWhiteSpace(name) ? throw new DomainException("Informe o nome da categoria.") : name.Trim();
        Type = type; Icon = icon; Color = color;
    }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public string Icon { get; private set; } = "tag";
    public string Color { get; private set; } = "#52606D";
    public bool IsArchived { get; private set; }
    public void Archive() { IsArchived = true; UpdatedAt = DateTimeOffset.UtcNow; }
}

public sealed class Transaction : Entity
{
    private Transaction() { }
    public Transaction(Guid userId, Guid categoryId, string description, decimal amount, TransactionType type,
        DateOnly occurredOn, TransactionStatus status = TransactionStatus.Confirmed)
    {
        if (userId == Guid.Empty || categoryId == Guid.Empty) throw new DomainException("Usuário e categoria são obrigatórios.");
        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 160) throw new DomainException("Descrição inválida.");
        if (amount <= 0 || amount > 999_999_999.99m) throw new DomainException("O valor deve ser maior que zero e estar no limite permitido.");
        UserId = userId; CategoryId = categoryId; Description = description.Trim();
        Amount = Money.Round(amount); Type = type; OccurredOn = occurredOn; Status = status;
    }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateOnly OccurredOn { get; private set; }
    public string? Note { get; private set; }
    public string? PaymentMethod { get; private set; }
}

public sealed class Budget : Entity
{
    private Budget() { }
    public Budget(Guid userId, int year, int month, decimal limit, Guid? categoryId = null)
    {
        if (userId == Guid.Empty || year is < 2000 or > 2200 || month is < 1 or > 12 || limit <= 0)
            throw new DomainException("Dados de orçamento inválidos.");
        UserId = userId; Year = year; Month = month; Limit = Money.Round(limit); CategoryId = categoryId;
    }
    public Guid UserId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal Limit { get; private set; }
    public BudgetHealth Health(decimal spent)
    {
        var ratio = Limit == 0 ? 1m : spent / Limit;
        return ratio >= 1m ? BudgetHealth.Exceeded : ratio >= .9m ? BudgetHealth.NearLimit : ratio >= .75m ? BudgetHealth.Attention : BudgetHealth.Normal;
    }
}

public static class Money
{
    public static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.ToEven);
    public static decimal Balance(IEnumerable<Transaction> transactions) => Money.Round(transactions
        .Where(x => x.Status == TransactionStatus.Confirmed)
        .Sum(x => x.Type == TransactionType.Income ? x.Amount : -x.Amount));
}

public sealed class DomainException(string message) : Exception(message);
