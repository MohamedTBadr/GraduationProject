# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY *.sln ./

# Copy all projects
COPY PAL/*.csproj PAL/
COPY BLL/*.csproj BLL/
COPY DAL/*.csproj DAL/
COPY Common/*.csproj Common/

# Restore dependencies for the solution
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Publish only the startup project (API)
RUN dotnet publish PAL/PAL.csproj -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "API.PAL.dll"]
