# ======================
# 🏗️ Build stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file
COPY *.sln ./

# Copy all project files
COPY PAL/*.csproj PAL/
COPY BLL/*.csproj BLL/
COPY DAL/*.csproj DAL/
COPY Common/*.csproj Common/

# Restore all dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Publish only the startup project (PAL)
RUN dotnet publish PAL/PAL.csproj -c Release -o /app/publish

# ======================
# 🚀 Runtime stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Run the Web API
ENTRYPOINT ["dotnet", "PAL.dll"]
