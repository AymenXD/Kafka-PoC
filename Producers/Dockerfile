FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Restore Stage
WORKDIR /src
COPY ["Producers.csproj", "Producers/"]
RUN dotnet restore "Producers/Producers.csproj"

WORKDIR /src/Producers
COPY . .

# Build Stage
RUN dotnet build "Producers.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "Producers.csproj" -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 
ENV ASPNETCORE_HTTP_PORTS=5131
EXPOSE 5131
WORKDIR /app
COPY --from=build /src/Producers/Certificates /app/Certificates
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "Producers.dll" ]


