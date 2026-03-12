FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WordsNote.API/WordsNote.API.csproj", "WordsNote.API/"]
COPY ["WordsNote.Application/WordsNote.Application.csproj", "WordsNote.Application/"]
COPY ["WordsNote.Domain/WordsNote.Domain.csproj", "WordsNote.Domain/"]
COPY ["WordsNote.Infrastructure/WordsNote.Infrastructure.csproj", "WordsNote.Infrastructure/"]
RUN dotnet restore "WordsNote.API/WordsNote.API.csproj"
COPY . .
WORKDIR "/src/WordsNote.API"
RUN dotnet build "WordsNote.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WordsNote.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WordsNote.API.dll"]
