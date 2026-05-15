# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy Solution file and restore dependencies
COPY AlgoSphere.slnx ./
COPY src/AlgoSphere.Domain/AlgoSphere.Domain.csproj src/AlgoSphere.Domain/
COPY src/AlgoSphere.Application/AlgoSphere.Application.csproj src/AlgoSphere.Application/
COPY src/AlgoSphere.Infrastructure/AlgoSphere.Infrastructure.csproj src/AlgoSphere.Infrastructure/
COPY src/AlgoSphere.Api/AlgoSphere.Api.csproj src/AlgoSphere.Api/

RUN dotnet restore src/AlgoSphere.Api/AlgoSphere.Api.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/AlgoSphere.Api/AlgoSphere.Api.csproj -c Release -o out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "AlgoSphere.Api.dll"]
