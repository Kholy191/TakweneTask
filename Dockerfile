# Build and publish the API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so this layer is cached until a project file changes.
COPY Takwene.Domain/Takwene.Domain.csproj Takwene.Domain/
COPY Takwene.Application/Takwene.Application.csproj Takwene.Application/
COPY Takwene.Infrastructure/Takwene.Infrastructure.csproj Takwene.Infrastructure/
COPY Takwene/Takwene.API.csproj Takwene/
RUN dotnet restore Takwene/Takwene.API.csproj

COPY . .
RUN dotnet publish Takwene/Takwene.API.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Run on the slim ASP.NET runtime as the image's non-root user, plain HTTP on 8080
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Takwene.API.dll"]
