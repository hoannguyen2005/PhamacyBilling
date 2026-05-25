FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy the csproj using subfolder path
COPY ["PharmacyBillingService/PharmacyBillingService.csproj", "PharmacyBillingService/"]
RUN dotnet restore "PharmacyBillingService/PharmacyBillingService.csproj"

# Copy all files
COPY PharmacyBillingService/ PharmacyBillingService/
WORKDIR "/src/PharmacyBillingService"
RUN dotnet build "PharmacyBillingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PharmacyBillingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PharmacyBillingService.dll"]
