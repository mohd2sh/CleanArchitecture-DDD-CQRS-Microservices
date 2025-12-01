FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy everything
COPY . .

# Restore and build
WORKDIR "/src/src/Services/WorkOrders.Service/WorkOrders.Service.Api"
RUN dotnet restore "WorkOrders.Service.Api.csproj"
RUN dotnet build "WorkOrders.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WorkOrders.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkOrders.Service.Api.dll"]
