FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта
COPY ["FlowerApi.csproj", "."]

# Восстанавливаем зависимости
RUN dotnet restore

# Копируем весь код
COPY . .

# Собираем приложение
RUN dotnet publish -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Открываем порт
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Запускаем
ENTRYPOINT ["dotnet", "FlowerApi.dll"]