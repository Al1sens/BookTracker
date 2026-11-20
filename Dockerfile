# ШАГ 1: Берём SDK .NET 10 для сборки проекта
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проекта и восстанавливаем пакеты
COPY *.csproj ./
RUN dotnet restore

# Копируем весь остальной код (Controllers, Models, Data, wwwroot) и собираем
COPY . ./
RUN dotnet publish -c Release -o /app

# ШАГ 2: Берём лёгкий Runtime .NET 10 для запуска готового сервера
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Настраиваем порт 8080 для облака
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Запускаем скомпилированный проект
ENTRYPOINT ["dotnet", "BookTracker.dll"]