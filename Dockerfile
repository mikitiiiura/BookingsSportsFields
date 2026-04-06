# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BookingsSportsFields.sln .
COPY BookingsSportsFields/BookingsSportsFields.csproj BookingsSportsFields/
COPY BookingsSportsFields.Application/BookingsSportsFields.Application.csproj BookingsSportsFields.Application/
COPY BookingsSportsFields.DataAccess/BookingsSportsFields.DataAccess.csproj BookingsSportsFields.DataAccess/

RUN dotnet restore BookingsSportsFields/BookingsSportsFields.csproj

COPY BookingsSportsFields/ BookingsSportsFields/
COPY BookingsSportsFields.Application/ BookingsSportsFields.Application/
COPY BookingsSportsFields.DataAccess/ BookingsSportsFields.DataAccess/

RUN dotnet publish BookingsSportsFields/BookingsSportsFields.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render задає PORT; слухаємо 0.0.0.0
ENTRYPOINT ["sh", "-c", "dotnet BookingsSportsFields.dll --urls http://0.0.0.0:${PORT:-8080}"]
