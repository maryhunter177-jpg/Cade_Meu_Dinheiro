# Testes

Os testes unitários cobrem cálculo de saldo, status de orçamento, valores extremos, arredondamento bancário e propriedades do hashing. A pipeline coleta cobertura XPlat.

Próxima camada obrigatória antes de produção: testes de API com `WebApplicationFactory`, PostgreSQL efêmero/Testcontainers e cenários de ownership entre dois usuários, token expirado, concorrência e falha de banco.
