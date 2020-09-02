# Setup Environment
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ./ ./
WORKDIR /app/FinanceAPI/
RUN dotnet restore

# Build
WORKDIR /app/FinanceAPI/
RUN dotnet publish -c Release -o /app/FinanceAPI/deploy

# Generate runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/FinanceAPI/deploy .
EXPOSE 80
ENTRYPOINT ["dotnet", "FinanceAPI.dll"]