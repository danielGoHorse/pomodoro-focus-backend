FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "Pomodoro.Api.dll"]
