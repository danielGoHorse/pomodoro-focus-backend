# Base image para runtime..
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagem para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo pro container
COPY . .

# Restaura os pacotes
RUN dotnet restore

# Publica o projeto em modo Release
RUN dotnet publish -c Release -o /app/publish

# Imagem final (runtime apenas)
FROM base AS final
WORKDIR /app

# Copia o build publicado pra imagem final
COPY --from=build /app/publish .

# Seta a porta padrão do Railway
ENV ASPNETCORE_URLS=http://+:8080

# Starta a aplicação
ENTRYPOINT ["dotnet", "Pomodoro.Api.dll"]
