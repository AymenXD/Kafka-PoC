FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Restore Stage
WORKDIR /src
COPY ["DataGeneration.csproj", "DataGeneration/"]
RUN dotnet restore "DataGeneration/DataGeneration.csproj"

WORKDIR /src/DataGeneration
COPY . .

# Build Stage
RUN dotnet build "DataGeneration.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "DataGeneration.csproj" -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 
ENV ASPNETCORE_HTTP_PORTS=5247
EXPOSE 5247
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "DataGeneration.dll" ]


