# ======================
# 🏗️ Build stage
# ======================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file
COPY *.sln ./

# Fix: also copy the dcproj so dotnet restore doesn't fail
COPY *.dcproj ./

# Copy all project files
COPY Domain/*.csproj Domain/
COPY Infrastructure/*.csproj Infrastructure/
COPY Application/*.csproj Application/
COPY Shared/*.csproj Shared/
COPY Web.Api/*.csproj Web.Api/
COPY Gateway.API/*.csproj Gateway.API/
# Restore all dependencies
RUN dotnet restore "API.sln"

# Copy the rest of the source code
COPY . .

# Fix: publish the correct startup project
RUN dotnet publish Web.Api/Web.Api.csproj -c Release -o /app/publish

# ======================
# 🚀 Runtime stage
# ======================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Run the Web API
ENTRYPOINT ["dotnet", "Web.Api.dll"]