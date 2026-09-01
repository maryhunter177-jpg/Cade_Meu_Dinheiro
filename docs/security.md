# Segurança e privacidade

- Senhas: PBKDF2-HMAC-SHA512, salt aleatório de 128 bits e 210 mil iterações.
- JWT: HMAC-SHA256, audiência/emissor validados, 15 minutos e clock skew de 30 segundos.
- Autorização: ownership aplicado em todas as queries da camada Application.
- Entrada: limites de tamanho/valor no domínio e validação amigável no caso de uso.
- API: HTTPS, CORS allowlist, rate limit no login/cadastro, headers anti-sniff/frame e Problem Details sem stack trace.
- Logs: erros internos usam apenas trace ID; senha, token e valores financeiros não são registrados.
- Segredos: somente variáveis de ambiente/user-secrets. Valores versionados são placeholders sem credencial real.

Antes da produção, implementar refresh-token rotativo com reutilização detectada, verificação/recuperação de e-mail, proteção distribuída contra brute force e gestão formal de consentimento/exclusão LGPD.
