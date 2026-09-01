# Banco de dados

PostgreSQL 16 com EF Core. A migration inicial cria `users`, `categories`, `transactions` e `budgets`.

- Dinheiro: `numeric(18,2)`.
- Consultas mensais: índice `(user_id, occurred_on)`.
- Relatórios por categoria: índice `(user_id, category_id, occurred_on)`.
- Categoria usada por transação tem exclusão restrita; arquivamento preserva histórico.
- E-mail é único e normalizado para minúsculas.

Aplicar migrations:

```powershell
dotnet ef database update --project src/CadeMeuDinheiro.Infrastructure --startup-project src/CadeMeuDinheiro.Api
```
