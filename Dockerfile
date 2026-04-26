# Use .NET 9 SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything and publish the API
COPY . ./
RUN dotnet publish GodGuidesUs.Api/GodGuidesUs.Api.csproj -c Release -o out

# Use .NET 9 runtime to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Tell Render to use port 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GodGuidesUs.Api.dll"]