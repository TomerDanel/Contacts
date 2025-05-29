FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore dependencies for the main project (this will restore referenced projects too)
RUN dotnet restore Contacts/Contacts.csproj

# Build and publish the application
RUN dotnet publish Contacts/Contacts.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Expose port 8080 to match what the app is listening on
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Set entry point with debug info
ENTRYPOINT ["sh", "-c", "echo 'Starting application...' && ls -la && dotnet Contacts.dll"]