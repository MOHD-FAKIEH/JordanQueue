FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/JordanQueue.Api/JordanQueue.Api.csproj", "src/JordanQueue.Api/"]
COPY ["src/JordanQueue.Application/JordanQueue.Application.csproj", "src/JordanQueue.Application/"]
COPY ["src/JordanQueue.Domain/JordanQueue.Domain.csproj", "src/JordanQueue.Domain/"]
COPY ["src/JordanQueue.Infrastructure/JordanQueue.Infrastructure.csproj", "src/JordanQueue.Infrastructure/"]
RUN dotnet restore "src/JordanQueue.Api/JordanQueue.Api.csproj"
COPY . .
WORKDIR "/src/src/JordanQueue.Api"
RUN dotnet build "JordanQueue.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JordanQueue.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "JordanQueue.Api.dll"]
