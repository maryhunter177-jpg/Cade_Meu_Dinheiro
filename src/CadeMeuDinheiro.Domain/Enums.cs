namespace CadeMeuDinheiro.Domain;

public enum TransactionType { Income = 1, Expense = 2 }
public enum TransactionStatus { Pending = 1, Confirmed = 2, Cancelled = 3 }
public enum BudgetHealth { Normal = 1, Attention = 2, NearLimit = 3, Exceeded = 4 }
