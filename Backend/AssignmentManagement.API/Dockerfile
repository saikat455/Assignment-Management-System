
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AssignmentManagement.Domain/*.csproj AssignmentManagement.Domain/
COPY AssignmentManagement.Application/*.csproj AssignmentManagement.Application/
COPY AssignmentManagement.Infrastructure/*.csproj AssignmentManagement.Infrastructure/
COPY AssignmentManagement.API/*.csproj AssignmentManagement.API/
RUN dotnet restore AssignmentManagement.API/AssignmentManagement.API.csproj

# Now copy everything else and publish.
COPY . .
RUN dotnet publish AssignmentManagement.API/AssignmentManagement.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# .NET 8's Linux container images run as a non-root user by default and
# listen on 8080 - no HTTPS certificate needed inside the container. Put a
# reverse proxy (nginx, Azure App Service, IIS ARR, etc.) in front for TLS
# in a real deployment.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AssignmentManagement.API.dll"]