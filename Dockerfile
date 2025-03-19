# Usa a imagem oficial do .NET SDK para build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copia o csproj e restaura as dependências
COPY *.csproj ./
RUN dotnet restore

# Copia tudo e faz o build em Release
COPY . ./
RUN dotnet publish -c Release -o out

# Usa a imagem oficial do ASP.NET para rodar
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expõe a porta 80 e define o entrypoint
EXPOSE 80
ENTRYPOINT ["dotnet", "SeuProjeto.dll"]