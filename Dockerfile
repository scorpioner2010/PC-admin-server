# --------------------------
# Build stage
# --------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копіюємо всі файли
COPY . .

# Переходимо до каталогу з проєктом
WORKDIR /app/dropdudeAPI

# Відновлення залежностей і публікація
RUN dotnet restore dropdudeAPI.csproj
RUN dotnet publish dropdudeAPI.csproj -c Release -o /app/out

# --------------------------
# Runtime stage
# --------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out ./

# Важливо для Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "DropDudeAPI.dll"]