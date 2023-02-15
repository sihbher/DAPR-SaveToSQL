FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build the app
COPY . .

# Compile the app
RUN dotnet build --configuration Release --output out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the compiled app from the build stage
COPY --from=build /app/out ./

# Expose the port the app is listening on
EXPOSE 4000

# Set the app as the command to run when the container starts
ENTRYPOINT ["dotnet", "DaprSaveToSql.dll"]