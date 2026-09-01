using CadeMeuDinheiro.Domain;
using Microsoft.EntityFrameworkCore;

namespace CadeMeuDinheiro.Application;

public sealed class AuthService(IAppDbContext db, IPasswordService passwords, ITokenService tokens)
{
    public async Task<AuthTokens> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        ValidatePassword(request.Password);
        if (await db.Users.AnyAsync(x => x.Email == email, ct)) throw new ConflictException("Este e-mail já está cadastrado.");
        var user = new User(request.Name, email, passwords.Hash(request.Password));
        db.Add(user);
        foreach (var seed in DefaultCategories(user.Id)) db.Add(seed);
        await db.SaveChangesAsync(ct);
        return tokens.Create(user);
    }

    public async Task<AuthTokens> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email && x.IsActive, ct);
        if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
            throw new AppValidationException("E-mail ou senha inválidos.");
        return tokens.Create(user);
    }

    private static void ValidatePassword(string value)
    {
        if (value.Length < 10 || !value.Any(char.IsUpper) || !value.Any(char.IsLower) || !value.Any(char.IsDigit))
            throw new AppValidationException("A senha deve ter ao menos 10 caracteres, letra maiúscula, minúscula e número.");
    }

    private static IEnumerable<Category> DefaultCategories(Guid id)
    {
        yield return new(id, "Salário", TransactionType.Income, "wallet", "#13A875");
        yield return new(id, "Moradia", TransactionType.Expense, "home", "#52606D");
        yield return new(id, "Alimentação", TransactionType.Expense, "food", "#F4B740");
        yield return new(id, "Transporte", TransactionType.Expense, "car", "#3E7CB1");
        yield return new(id, "Saúde", TransactionType.Expense, "heart", "#D64550");
    }
}

public sealed class FinanceService(IAppDbContext db)
{
    public async Task<TransactionResponse> CreateAsync(Guid userId, TransactionRequest request, CancellationToken ct)
    {
        var category = await db.Categories.SingleOrDefaultAsync(x => x.Id == request.CategoryId && x.UserId == userId && !x.IsArchived, ct)
            ?? throw new AppValidationException("Categoria inválida.");
        if (category.Type != request.Type) throw new AppValidationException("O tipo da categoria não corresponde ao tipo da transação.");
        var transaction = new Transaction(userId, request.CategoryId, request.Description, request.Amount, request.Type, request.OccurredOn);
        db.Add(transaction); await db.SaveChangesAsync(ct);
        return Map(transaction);
    }

    public async Task<Page<TransactionResponse>> ListAsync(Guid userId, int page, int pageSize, TransactionType? type, string? search, CancellationToken ct)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
        var query = db.Transactions.Where(x => x.UserId == userId);
        if (type.HasValue) query = query.Where(x => x.Type == type);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Description, pattern));
        }
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.OccurredOn).ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new TransactionResponse(x.Id, x.CategoryId, x.Description, x.Amount, x.Type, x.OccurredOn)).ToListAsync(ct);
        return new(items, page, pageSize, total);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var transaction = await db.Transactions.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct)
            ?? throw new NotFoundException("Transação não encontrada.");
        db.Remove(transaction); await db.SaveChangesAsync(ct);
    }

    public async Task<DashboardResponse> DashboardAsync(Guid userId, int year, int month, CancellationToken ct)
    {
        var start = new DateOnly(year, month, 1); var end = start.AddMonths(1);
        var items = await db.Transactions.Where(x => x.UserId == userId && x.Status == TransactionStatus.Confirmed && x.OccurredOn >= start && x.OccurredOn < end)
            .OrderByDescending(x => x.OccurredOn).ToListAsync(ct);
        var income = Money.Round(items.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount));
        var expenses = Money.Round(items.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount));
        return new(Money.Round(income - expenses), income, expenses, Money.Round(income - expenses), items.Take(5).Select(Map).ToList());
    }

    private static TransactionResponse Map(Transaction x) => new(x.Id, x.CategoryId, x.Description, x.Amount, x.Type, x.OccurredOn);
}
