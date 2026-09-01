# API

Todas as respostas de erro seguem RFC 7807 (`application/problem+json`) e incluem `traceId`. Recursos financeiros exigem Bearer JWT e sempre derivam o usuário do claim `sub`; nenhum endpoint aceita `userId` do cliente.

## Autenticação

- `POST /api/auth/register`: nome, e-mail e senha forte.
- `POST /api/auth/login`: e-mail e senha.

## Transações

- `GET /api/transactions?page=1&pageSize=20&type=2&search=mercado`
- `POST /api/transactions`
- `DELETE /api/transactions/{id}`

`pageSize` é limitado a 100. Exclusão de ID pertencente a outro usuário responde como não encontrado, evitando enumeração de recursos.

## Dashboard

`GET /api/dashboard?year=2026&month=9` retorna saldo, receitas, despesas, economia e cinco movimentações recentes confirmadas.
