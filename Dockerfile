# Use a .NET runtime as the base image (smaller than the SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

# Set the working directory inside the container
WORKDIR /app

# Expose the necessary ports
EXPOSE 80
EXPOSE 8080
EXPOSE 443

# Use the SDK image only for building and publishing the application
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Set the working directory inside the container
WORKDIR /src

# Copy the project file into the container
COPY ["DotNet.Docker.csproj", "./"]

# Restore the NuGet dependencies
RUN dotnet restore "./DotNet.Docker.csproj"

# Copy the remaining code into the container
COPY . ./

# Build the application
RUN dotnet build "DotNet.Docker.csproj" -c Release -o /app/build

# Publish the application to the publish directory
FROM build AS publish
RUN dotnet publish "DotNet.Docker.csproj" -c Release -o /app/publish

# Now, use the base image with only the runtime for execution
FROM base AS final

# Set the working directory
WORKDIR /app

# Copy the files from the publish stage to the final image
COPY --from=publish /app/publish ./

# Command to start the application when the container runs
ENTRYPOINT ["dotnet", "DotNet.Docker.dll"]
