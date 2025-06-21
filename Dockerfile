# Use the .NET 8 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files and restore dependencies
COPY WEB-BOOK-API.sln .
COPY src/BookStore.API/BookStore.API.csproj src/BookStore.API/
COPY src/BookStore.Application/BookStore.Application.csproj src/BookStore.Application/
COPY src/BookStore.Domain/BookStore.Domain.csproj src/BookStore.Domain/
COPY src/BookStore.Infrastructure/BookStore.Infrastructure.csproj src/BookStore.Infrastructure/
# If you have test projects, you might want to copy them too and run tests
# COPY tests/BookStore.Tests/BookStore.Tests.csproj tests/BookStore.Tests/

RUN dotnet restore "WEB-BOOK-API.sln"

# Copy the rest of the source code
COPY . .
WORKDIR /app

# Publish the API project
RUN dotnet publish "src/BookStore.API/BookStore.API.csproj" -c Release -o /app/publish

# Use the .NET 8 ASP.NET runtime image for the final, smaller image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set the entrypoint to run the application
ENTRYPOINT ["dotnet", "BookStore.API.dll"] 