FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY FeatureFusion/FeatureFusion.csproj FeatureFusion/
RUN dotnet restore FeatureFusion/FeatureFusion.csproj
COPY . .
RUN dotnet publish FeatureFusion/FeatureFusion.csproj -c Release -o /app/publish /p:UseAppHost=false /p:SelfContained=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
RUN apk add --no-cache icu-libs curl
COPY --from=build /app/publish .
EXPOSE 8080
EXPOSE 8081
ENTRYPOINT ["dotnet", "FeatureFusion.dll"]
