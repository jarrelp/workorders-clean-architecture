# Build stage: restore before copying the sources, so the package layer stays cached
# until a project file changes.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/WorkOrders.Domain/WorkOrders.Domain.csproj src/WorkOrders.Domain/
COPY src/WorkOrders.Application/WorkOrders.Application.csproj src/WorkOrders.Application/
COPY src/WorkOrders.Infrastructure/WorkOrders.Infrastructure.csproj src/WorkOrders.Infrastructure/
COPY src/WorkOrders.Api/WorkOrders.Api.csproj src/WorkOrders.Api/
RUN dotnet restore src/WorkOrders.Api/WorkOrders.Api.csproj

COPY src/ src/
RUN dotnet publish src/WorkOrders.Api/WorkOrders.Api.csproj --configuration Release --no-restore --output /app

# Runtime stage: the ASP.NET Core runtime and the published app only, running as the
# image's non-root user. The API listens on port 8080 (the image default).
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# SQLite needs a writable directory. Mount a volume on it to keep the data between containers.
RUN mkdir /app/data && chown "$APP_UID" /app/data
ENV ConnectionStrings__WorkOrders="Data Source=/app/data/workorders.db"
VOLUME /app/data

COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "WorkOrders.Api.dll"]
