# Use the official .NET 6.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Use the official .NET 6.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the project file
COPY ["QdrantExample.csproj", "./"]

# Restore dependencies
RUN dotnet restore "QdrantExample.csproj"

# Copy the source code
COPY . ./

# Build the application
RUN dotnet build "QdrantExample.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "QdrantExample.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "QdrantExample.dll"]
