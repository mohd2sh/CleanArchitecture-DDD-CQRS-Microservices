FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Orchestration.Service/Orchestration.Service.csproj", "src/Services/Orchestration.Service/"]
COPY ["src/Core/CleanArchitecture.Core.Messaging/CleanArchitecture.Core.Messaging.csproj", "src/Core/CleanArchitecture.Core.Messaging/"]
COPY ["src/Contracts/CleanArchitecture.Cmms.Contracts.csproj", "src/Contracts/"]
RUN dotnet restore "src/Services/Orchestration.Service/Orchestration.Service.csproj"
COPY . .
WORKDIR "/src/src/Services/Orchestration.Service"
RUN dotnet build "Orchestration.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orchestration.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orchestration.Service.dll"]

