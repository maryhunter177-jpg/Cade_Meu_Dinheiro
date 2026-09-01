using CadeMeuDinheiro.Application;
using CadeMeuDinheiro.Domain;
using Microsoft.EntityFrameworkCore;

namespace CadeMeuDinheiro.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> UserSet => Set<User>();
    public DbSet<Category> CategorySet => Set<Category>();
    public DbSet<Transaction> TransactionSet => Set<Transaction>();
    public DbSet<Budget> BudgetSet => Set<Budget>();
    IQueryable<User> IAppDbContext.Users => UserSet;
    IQueryable<Category> IAppDbContext.Categories => CategorySet;
    IQueryable<Transaction> IAppDbContext.Transactions => TransactionSet;
    IQueryable<Budget> IAppDbContext.Budgets => BudgetSet;
    void IAppDbContext.Add<TEntity>(TEntity entity) => Add(entity);
    void IAppDbContext.Remove<TEntity>(TEntity entity) => Remove(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users"); b.HasKey(x => x.Id); b.HasIndex(x => x.Email).IsUnique();
            b.Property(x => x.Name).HasMaxLength(120); b.Property(x => x.Email).HasMaxLength(254);
            b.Property(x => x.PasswordHash).HasMaxLength(500); b.Property(x => x.Currency).HasMaxLength(3);
        });
        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("categories"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.UserId, x.Name, x.Type }).IsUnique();
            b.Property(x => x.Name).HasMaxLength(80); b.Property(x => x.Icon).HasMaxLength(40); b.Property(x => x.Color).HasMaxLength(9);
            b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Transaction>(b =>
        {
            b.ToTable("transactions"); b.HasKey(x => x.Id); b.HasIndex(x => new { x.UserId, x.OccurredOn });
            b.HasIndex(x => new { x.UserId, x.CategoryId, x.OccurredOn }); b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.Description).HasMaxLength(160); b.Property(x => x.Note).HasMaxLength(500); b.Property(x => x.PaymentMethod).HasMaxLength(60);
            b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Budget>(b =>
        {
            b.ToTable("budgets"); b.HasKey(x => x.Id); b.Property(x => x.Limit).HasPrecision(18, 2);
            b.HasIndex(x => new { x.UserId, x.Year, x.Month, x.CategoryId }).IsUnique();
            b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
