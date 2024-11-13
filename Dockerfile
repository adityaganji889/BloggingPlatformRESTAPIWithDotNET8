# Use the official ASP.NET runtime as a base image
# Use the official ASP.NET 8.0 runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BloggingPlatform.csproj", "./"]
RUN dotnet restore "./BloggingPlatform.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BloggingPlatform.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BloggingPlatform.csproj" -c Release -o /app/publish

# Final stage to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BloggingPlatform.dll"]