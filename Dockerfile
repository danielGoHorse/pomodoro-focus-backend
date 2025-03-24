# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Passa variáveis de ambiente no container
ENV ASPNETCORE_ENVIRONMENT=Production
ENV Spotify__ClientId=from-docker
ENV Spotify__ClientSecret=from-docker
ENV Spotify__RedirectUri=from-docker
ENV ConnectionStrings__DefaultConnection=from-docker

EXPOSE 80
ENTRYPOINT ["dotnet", "Pomodoro.Api.dll"]
