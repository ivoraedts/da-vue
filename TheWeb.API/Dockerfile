# Stage 1: Base - Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build - SDK for compiling
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
# Copy specifically the csproj first to leverage Docker caching
COPY ["TheWeb.API.csproj", "."]
RUN dotnet restore "./TheWeb.API.csproj"
# Copy everything else and build
COPY . .
RUN dotnet build "TheWeb.API.csproj" -c Release -o /app/build

# Stage 3: Publish - Prepare for final image
FROM build AS publish
RUN dotnet publish "TheWeb.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final - Lean runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TheWeb.API.dll"]
