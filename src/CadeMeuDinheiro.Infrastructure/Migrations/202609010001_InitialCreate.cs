using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CadeMeuDinheiro.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202609010001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    private static readonly string[] CategoryUniqueColumns = ["UserId", "Name", "Type"];
    private static readonly string[] BudgetUniqueColumns = ["UserId", "Year", "Month", "CategoryId"];
    private static readonly string[] TransactionDateColumns = ["UserId", "OccurredOn"];
    private static readonly string[] TransactionCategoryDateColumns = ["UserId", "CategoryId", "OccurredOn"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("users", table => new
        {
            Id = table.Column<Guid>(nullable: false), Name = table.Column<string>(maxLength: 120, nullable: false),
            Email = table.Column<string>(maxLength: 254, nullable: false), PasswordHash = table.Column<string>(maxLength: 500, nullable: false),
            Currency = table.Column<string>(maxLength: 3, nullable: false), EmailVerified = table.Column<bool>(nullable: false), IsActive = table.Column<bool>(nullable: false),
            CreatedAt = table.Column<DateTimeOffset>(nullable: false), UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
        }, constraints: table => table.PrimaryKey("PK_users", x => x.Id));
        migrationBuilder.CreateTable("categories", table => new
        {
            Id = table.Column<Guid>(nullable: false), UserId = table.Column<Guid>(nullable: false), Name = table.Column<string>(maxLength: 80, nullable: false),
            Type = table.Column<int>(nullable: false), Icon = table.Column<string>(maxLength: 40, nullable: false), Color = table.Column<string>(maxLength: 9, nullable: false),
            IsArchived = table.Column<bool>(nullable: false), CreatedAt = table.Column<DateTimeOffset>(nullable: false), UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_categories", x => x.Id); table.ForeignKey("FK_categories_users_UserId", x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateTable("budgets", table => new
        {
            Id = table.Column<Guid>(nullable: false), UserId = table.Column<Guid>(nullable: false), CategoryId = table.Column<Guid>(nullable: true), Year = table.Column<int>(nullable: false),
            Month = table.Column<int>(nullable: false), Limit = table.Column<decimal>(type: "numeric(18,2)", nullable: false), CreatedAt = table.Column<DateTimeOffset>(nullable: false), UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_budgets", x => x.Id); table.ForeignKey("FK_budgets_users_UserId", x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_budgets_categories_CategoryId", x => x.CategoryId, "categories", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateTable("transactions", table => new
        {
            Id = table.Column<Guid>(nullable: false), UserId = table.Column<Guid>(nullable: false), CategoryId = table.Column<Guid>(nullable: false), Description = table.Column<string>(maxLength: 160, nullable: false),
            Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false), Type = table.Column<int>(nullable: false), Status = table.Column<int>(nullable: false), OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
            Note = table.Column<string>(maxLength: 500, nullable: true), PaymentMethod = table.Column<string>(maxLength: 60, nullable: true), CreatedAt = table.Column<DateTimeOffset>(nullable: false), UpdatedAt = table.Column<DateTimeOffset>(nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_transactions", x => x.Id); table.ForeignKey("FK_transactions_users_UserId", x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_transactions_categories_CategoryId", x => x.CategoryId, "categories", "Id", onDelete: ReferentialAction.Restrict); });
        migrationBuilder.CreateIndex("IX_users_Email", "users", "Email", unique: true);
        migrationBuilder.CreateIndex("IX_categories_UserId_Name_Type", "categories", CategoryUniqueColumns, unique: true);
        migrationBuilder.CreateIndex("IX_budgets_CategoryId", "budgets", "CategoryId");
        migrationBuilder.CreateIndex("IX_budgets_UserId_Year_Month_CategoryId", "budgets", BudgetUniqueColumns, unique: true);
        migrationBuilder.CreateIndex("IX_transactions_CategoryId", "transactions", "CategoryId");
        migrationBuilder.CreateIndex("IX_transactions_UserId_OccurredOn", "transactions", TransactionDateColumns);
        migrationBuilder.CreateIndex("IX_transactions_UserId_CategoryId_OccurredOn", "transactions", TransactionCategoryDateColumns);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("budgets"); migrationBuilder.DropTable("transactions"); migrationBuilder.DropTable("categories"); migrationBuilder.DropTable("users");
    }
}
