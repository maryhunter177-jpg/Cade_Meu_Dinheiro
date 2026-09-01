# Arquitetura

## Decisões

A solução segue Clean Architecture com quatro limites concretos. O domínio não conhece banco, HTTP ou UI. Application depende apenas do domínio e concentra casos de uso. Infrastructure implementa persistência e criptografia. Api e App são adaptadores externos.

Não foi criado repositório genérico: `IAppDbContext` oferece o ponto de teste necessário e preserva projeções EF eficientes. CQRS, event bus e microserviços foram evitados porque não agregam valor ao MVP.

## Fluxo de uma transação

```text
JWT -> endpoint -> UserId autenticado -> FinanceService
                                      -> valida categoria do mesmo usuário
                                      -> entidade Transaction
                                      -> PostgreSQL
```

Datas financeiras usam `DateOnly`; auditoria usa `DateTimeOffset` em UTC. Valores usam `decimal(18,2)` e arredondamento `ToEven`. Receitas e despesas são armazenadas positivas; o tipo define o sinal no cálculo.

## Extensão

`IFinancialInsightsService` é apenas um contrato de extensão para inteligência financeira futura. Notificações, Open Finance e offline devem entrar como adaptadores, sem contaminar as regras atuais.
