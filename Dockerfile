FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/CadeMeuDinheiro.Api/CadeMeuDinheiro.Api.csproj
RUN dotnet publish src/CadeMeuDinheiro.Api/CadeMeuDinheiro.Api.csproj -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --home /app --gecos "" appuser && chown -R appuser /app
COPY --from=build --chown=appuser:appuser /app .
USER appuser
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CadeMeuDinheiro.Api.dll"]
