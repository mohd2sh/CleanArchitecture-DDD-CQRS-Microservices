FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy everything
COPY . .

# Restore and build
WORKDIR "/src/src/services/Technicians.Service/CleanArchitecture.Cmms.Api.Technicians"
RUN dotnet restore "CleanArchitecture.Cmms.Api.Technicians.csproj"
RUN dotnet build "CleanArchitecture.Cmms.Api.Technicians.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CleanArchitecture.Cmms.Api.Technicians.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitecture.Cmms.Api.Technicians.dll"]
