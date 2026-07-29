# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/AutoFinderAI.Domain/AutoFinderAI.Domain.csproj src/AutoFinderAI.Domain/
COPY src/AutoFinderAI.Application/AutoFinderAI.Application.csproj src/AutoFinderAI.Application/
COPY src/AutoFinderAI.Infrastructure/AutoFinderAI.Infrastructure.csproj src/AutoFinderAI.Infrastructure/
COPY src/AutoFinderAI.Api/AutoFinderAI.Api.csproj src/AutoFinderAI.Api/
RUN dotnet restore src/AutoFinderAI.Api/AutoFinderAI.Api.csproj

COPY src/ src/
RUN dotnet publish src/AutoFinderAI.Api/AutoFinderAI.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN useradd --uid 10001 --user-group --create-home appuser
COPY --from=build /app .
RUN mkdir -p /data /app/logs && chown -R appuser:appuser /app /data
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AutoFinderAI.Api.dll"]
