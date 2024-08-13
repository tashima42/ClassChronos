FROM mcr.microsoft.com/dotnet/sdk:8.0.303 AS build-env
WORKDIR /App

# Copy everything
COPY *.csproj ./
# Restore as distinct layers
RUN dotnet restore

# Copy everything else and build
COPY . ./
# Build and publish a release
RUN dotnet publish --no-restore -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
COPY --from=build-env /App/Data ./Data

ENTRYPOINT ["dotnet", "UTFClassAPI.dll"]